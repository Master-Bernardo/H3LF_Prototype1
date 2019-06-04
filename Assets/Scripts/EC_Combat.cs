using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/*
 * another unit Compnent responsible for combat, holds all attacks and all reaction on attacks, determines if the units gets the damage, evades or counterattacks
 */
public enum CombatState
{
    //melee
    //Idle, //the normal idle mode
    CombatIdle, //idle but ready for combat
    PreparingMeleeAttack, //the swing of a weapon, if not interrupted by a damage flinch or another action, it is followed by either a melee attack if it was sucessful or an attack flichnch if the enemy blocked sucessfuly or it just ends apruptly if the enemy defended normal or poorly
    FinishingMeleeAttack,
    //RecoveringFromMeleeAttack,
    PreparingBlock,
    //EndingBlock,
    Blocking,
    //Stunned, //certain attacks can set us stunned for a certain time
    BlockFlinching, //we flinch if we barely block, if we made a block of level 2
    AttackFlinching, //we flinch if we attacked but the block was masterfull (lvl 4) and it gives us a malus
    DamageFlinching, //we flinch if we get damage higher than a certain treshold - like in m&b

    Evading,
    //missile
    /*
    Drawing,
    Reloading,
    Aiming,//?
           //magic
    Casting,
    HoldingSpell,*/
}


[System.Serializable]
public class EC_Combat : EntityComponent
{

    //components of unit used by this component, are being set up at start
    EC_UnitAnimation unitAnimation;
    EC_Health health;
    EC_Movement movement;
    Transform myTransform;
    Collider myCollider;

    GameEntity currentTarget; //used for the attacks
    EC_Combat currentTargetsCombat;
    EC_UnitAI currentTargetsUnitAI;
    EC_Health currentTargetsHealth;

    public CombatState currentCombatState = CombatState.CombatIdle;

    
    CA_MeleeAttack currentAttack;
    CA_Block currentBlock;

    //instead of invokes we use our intern functions for this - performance optimisation
    //we just save the time at which we start the function and we check in update based on current state
    float invokeTime; //the time at which the function will be invoked




    public float damageFlinchTime = 0.5f;  //---where and how should we determine damage flinch?

    public bool drawDamageGizmo = true;
    bool drawDamageBoxGizmo = false;
    float framesRemainingToShowDamageBox = 15;
    [Tooltip("how many frames is the gizmo of our damagesphere visible?")]
    public float framesToShowDamageSphere = 15;
    Vector3 gizmoPosition;
    float gizmoRadius;
    Color gizmoColor;


    public override void SetUpEntityComponent(GameEntity entity)
    {
        base.SetUpEntityComponent(entity);

        unitAnimation = entity.unitAnimation;
        health = entity.health;
        movement = entity.movement;
        myTransform = entity.myTransform;
        myCollider = entity.GetComponent<Collider>();

        gizmoColor = myTransform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().material.color;
    }

    public override void UpdateEntityComponent(float deltaTime, float time)
    {
        if (time >= invokeTime)
        {

            switch (currentCombatState)
            {
                case (CombatState.PreparingMeleeAttack):
                    MA_DetermineAttackOutcome();
                    break;

                case (CombatState.FinishingMeleeAttack):
                    GoBackToIdle();
                    break;

                case (CombatState.AttackFlinching):
                    GoBackToIdle();
                    break;

                case (CombatState.PreparingBlock):
                    B_StartBlocking();
                    break;

                case (CombatState.BlockFlinching):
                    GoBackToIdle();
                    break;

                case (CombatState.Evading):
                    GoBackToIdle();
                    break;

                case (CombatState.DamageFlinching):
                    GoBackToIdle();
                    break;
            }
        }



    }





    #region meleeAttack

    //we choose who should be attacked and whit which attack
    public void MeleeAttack(GameEntity target, CA_MeleeAttack attack)
    {
        Profiler.BeginSample("MeleeAttackBeginn");

        currentAttack = attack;

        if (target != null)
        {
            currentTarget = target;
            currentTargetsUnitAI = currentTarget.unitAI;
        }


        Profiler.EndSample();

        movement.Stop();

        Profiler.BeginSample("PrepareMeleeAttack");
        MA_PrepareMeleeAttack();
        Profiler.EndSample();
    }

    public void AbortMeleeAttack()
    {
        if (currentCombatState == CombatState.PreparingMeleeAttack)
        {
            currentCombatState = CombatState.CombatIdle;
            unitAnimation.BackToIdle();
        }
    }

    //MA_ means the functions are the ones invoked during an attack

    //1. starts the swing animation, and the attack on a target - at the end of the swing, we check if the target is still in range and if he does not block
    void MA_PrepareMeleeAttack()
    {
        currentCombatState = CombatState.PreparingMeleeAttack;

       
        invokeTime = Time.time + currentAttack.prepareMeleeAttackTime;


        if (currentTargetsUnitAI != null) currentTargetsUnitAI.OnEnemySwingsWeaponOnMe();

        unitAnimation.PlayPrepareAttackAnimation(currentAttack.prepareMeleeAttackTime, currentAttack.animatorTriggerID);

        movement.Stop();

        //move the unit in the right range of the attack
        //movement.MoveTo(currentTarget.transform.position + (transform.position - currentTarget.transform.position).normalized * currentAttack.meleeRange);
        if (currentAttack.hasMovement)
        {
            //movement.MoveTo(transform.position + currentAttack.movementDirection*currentAttack.movementDistance);
            movement.MoveTo(myTransform.position + myTransform.TransformDirection(currentAttack.movementDirection * currentAttack.movementDistance));
        }
    }

    //2. weapon lands on the target- what now?
    void MA_DetermineAttackOutcome()
    {
        //TODO make a bounds check here - draw the bounds with on Gizmo Draw, the bounds are saved in the CA_Attack object

        List<GameEntity> surroundingEnemies = new List<GameEntity>();

        int layerMask1 = 1 << 12;
        int layerMask2 = 1 << 13;
        int layerMask = layerMask1 | layerMask2; ;

        //sphere for now, later box
        Collider[] nearestUnits = Physics.OverlapSphere(myTransform.TransformPoint(currentAttack.hitSpherePosition), currentAttack.hitSphereRadius, layerMask);

        drawDamageBoxGizmo = true;
        gizmoPosition = myTransform.TransformPoint(currentAttack.hitSpherePosition);
        gizmoRadius = currentAttack.hitSphereRadius;

        currentTarget = null;

        if (nearestUnits.Length > 0)
        {
            if (nearestUnits[0] != myCollider)
            {
                currentTarget = nearestUnits[0].GetComponent<GameEntity>();
            }
            else if (nearestUnits.Length > 1)
            {
                if (nearestUnits[1] != null)
                {
                    currentTarget = nearestUnits[1].GetComponent<GameEntity>();
                }
            }
        }


        if (currentTarget != null)
        {
            //TODO if the block is sucessful is dependant on skills levels
            
            //for now only one enemy

            currentTargetsCombat = currentTarget.combat;
            currentTargetsUnitAI = currentTarget.unitAI;
            currentTargetsHealth = currentTarget.health;

            if (currentTargetsCombat != null)
            {
                //TODO is the vector angle check the most performant one?
                if (currentTargetsCombat.currentCombatState == CombatState.Blocking && Vector3.Angle(currentTarget.transform.forward, -myTransform.forward) < currentTargetsCombat.currentBlock.blockCoveringAngle)
                {
                    MA_AttackFlinch();
                    currentTargetsUnitAI.OnEnemyWasBlockedByMe();
                }
                else
                {
                    MA_FinishAttack();
                }
            }
            else //if the target does not have any combat, just deal the damage
            {
                currentTargetsHealth.TakeDamage(currentAttack.meleeDamage);
            }
        }
        else
        {
            GoBackToIdle();
        }
    }

    //3.a if the defender did not defend, we finish the attack
    void MA_FinishAttack()
    {
        currentTargetsCombat.OnEnemyLandsHitOnMe();

        currentTargetsHealth.TakeDamage(currentAttack.meleeDamage);
        currentTargetsCombat.DamageFlinch();

        currentCombatState = CombatState.FinishingMeleeAttack;
        unitAnimation.PlayFinishAttackAnimation(currentAttack.finishAttackTime);
        invokeTime =  currentAttack.finishAttackTime;
    }

    //3.b if the defender defended masterfully, we flinch by his defense
    void MA_AttackFlinch()
    {
        currentCombatState = CombatState.AttackFlinching;
        unitAnimation.PlayAttackFlinchAnimation(currentAttack.attackFlinchTime);
        invokeTime = currentAttack.attackFlinchTime;
    }

    #endregion

    #region block

    public void Block(CA_Block block)
    {
        currentBlock = block;

        movement.Stop();

        B_PrepareBlocking();
    }

    //the same as go back to idle
    public void StopBlocking()
    {
        GoBackToIdle();
    }

    //B_ means the functions are the ones invoked during an attack

    //1. beginn to move into the block position
    void B_PrepareBlocking()
    {
        currentCombatState = CombatState.PreparingBlock;
        unitAnimation.PlayPrepareBlockAnimation(currentBlock.prepareBlockingTime, currentBlock.animatorTriggerID);
        invokeTime =  currentBlock.prepareBlockingTime;
    }

    //2. start performing the block
    void B_StartBlocking()
    {
        currentCombatState = CombatState.Blocking;
        unitAnimation.PlayBlockAnimation();
    }

    //3.a if we blocked, but no really good
    public void B_BlockFlinch()
    {
        currentCombatState = CombatState.BlockFlinching;
        unitAnimation.PlayBlockFlinchAnimation(currentBlock.blockFlinchTime);
        invokeTime = currentBlock.blockFlinchTime;
    }

    #endregion

    #region evade

    public void Evade(CA_Evade evade)
    {
        movement.MoveTo(myTransform.position + myTransform.TransformDirection(evade.direction));

        currentCombatState = CombatState.Evading;
        invokeTime = evade.evadeTime;
    }

    #endregion

    void DamageFlinch()
    {
        currentCombatState = CombatState.DamageFlinching;
        unitAnimation.PlayDamageFlinchAnimation(damageFlinchTime);
        invokeTime =  damageFlinchTime;
    }

    void GoBackToIdle()
    {
        currentCombatState = CombatState.CombatIdle;
        unitAnimation.BackToIdle();
    }

    private void OnDrawGizmos()
    {
        if (drawDamageGizmo)
        {
            if (drawDamageBoxGizmo)
            {

                Gizmos.color = gizmoColor;
                Gizmos.DrawWireSphere(gizmoPosition, gizmoRadius);

                framesRemainingToShowDamageBox--;
                if (framesRemainingToShowDamageBox == 0)
                {
                    framesRemainingToShowDamageBox = framesToShowDamageSphere;
                    drawDamageBoxGizmo = false;
                }
            }
        }
    }



    /*public void OnEnemySwingsWeaponOnMe()
    {

    }*/


    public void OnEnemyLandsHitOnMe()
    {
       /* //Debug.Log("enemy lands hit on me");

        //we make a damage flicnch - that neans we are stunned for a short period of time because of pain like in m&b warband
        DamageFlinch();
        health.TakeDamage(meleeDamage);
        Debug.Log("au");*/
    }
    //gets called by the enemy if we block his attack
    public void OnEnemyWasBlockedByMe()
    {
        B_BlockFlinch(); 
    }

    

  
}
