using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;


/* 
 * Every unit has a few behaviours - special fighting styles fleeing or harvesting of something - the UnitAI switches between the different behaviours, like an extension of the unit ai class
 */
[System.Serializable]
public class UnitBehaviour
{
    protected GameEntity entity;

    //chached parameter we will certainly acess
    Transform myTransform;

    public UnitBehaviour(GameEntity entity, Transform transform)
    {
        this.entity = entity;
    }

    


    //gets called by UnitAI
    public virtual void BehaviourUpdate()
    {
       // Debug.Log("base class");
    }


    public void OnEnemySwingsWeaponAtMe()
    {

    }

    public void OnEnemyWasBlockedByMe()
    {

    }
}

public class FleeBehaviour : UnitBehaviour
{
    public FleeBehaviour(GameEntity entity, Transform transform):base(entity,transform)
    {

    }

    public override void BehaviourUpdate()
    {
        //Debug.Log("flee class");
    }
}

//this behaviour is specialized for our swordfighter

[System.Serializable]
public class MeleeAttackBehaviour: UnitBehaviour
{
    public enum MeleeAttackBehaviourState
    {
        Idle,
        MovingToNearestEnemy,
        InMeleeFight
    }

    public MeleeAttackBehaviourState meleeAttackBehaviourState = MeleeAttackBehaviourState.Idle;

    EC_Movement movement;
    EC_Combat combat;
    EC_Sensing sensing;
    EC_UnitAI unitAI;
    GameEntity nearestEnemy;
    GameEntity nearestEnemyLastFrame; //for optimisation

    float rangeToEnterMeleeCombatSquared = 8f; //this is the range in which the enemy beginns to fight

    //update the moveToOrder every few seconds - improve performance by not calling MoveTo chan making the distanceCheck too often
    float updateDestinationInterval = 0.2f;
    float nextUpdateDestinationTime;

    //current variables stored for performance optimisation
    float currentSquaredDistanceToNearestEnemy;
    UnitStance currentStance;

    int idleStance = 0;
    int sabreStance = 1;
    int spearStance = 2;


    //cache this data for every update
    Transform nearestEnemysTransform;
    Vector3 nearestEnemyPosition;
    Vector3 myTransformPosition;
    bool enemyIsNull;

    public MeleeAttackBehaviour(GameEntity entity, Transform transform) : base(entity,transform)
    {
        movement = entity.movement;
        combat = entity.combat;
        sensing = entity.sensing;
        unitAI = entity.unitAI;

        nextUpdateDestinationTime = Random.Range(0, updateDestinationInterval);
    }

    public override void BehaviourUpdate()
    {
        //Profiler.BeginSample("BehaviourUpdate");
        #region debug
        if (Input.GetKeyDown(KeyCode.B))
        {
            combat.Block(currentStance.blocks[0]);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            combat.B_BlockFlinch();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            combat.Evade(currentStance.evades[1]);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            combat.MeleeAttack(null, currentStance.attacks[1]);
        }
        #endregion


        //cache some variables for later
        nearestEnemy = sensing.nearestEnemy;
        currentStance = unitAI.currentStance;
        myTransformPosition = entity.myTransform.position;
        enemyIsNull = nearestEnemy == null;


        //1. look at the enemy, if no enemy present -> continue looking in the movement direction


        if (!enemyIsNull)
        {
            //but only if something chenged
            if (nearestEnemyLastFrame != nearestEnemy)
            {
                nearestEnemysTransform = nearestEnemy.myTransform;
                movement.LookAt(nearestEnemysTransform);
            }

            nearestEnemyPosition = nearestEnemysTransform.position;
            currentSquaredDistanceToNearestEnemy = (nearestEnemyPosition - myTransformPosition).sqrMagnitude;
        }
        else
        {
            if (nearestEnemyLastFrame != nearestEnemy)
            {
                movement.StopLookAt();
            }
           
        }
        
        switch (meleeAttackBehaviourState)
        {
            //2. if theres an enemy -> walk towards him
            case MeleeAttackBehaviourState.Idle:

                if (!enemyIsNull)
                {
                    movement.MoveTo(nearestEnemyPosition);
                    meleeAttackBehaviourState = MeleeAttackBehaviourState.MovingToNearestEnemy;
                }

                break;

            case MeleeAttackBehaviourState.MovingToNearestEnemy:

                //check if the enemy is still alive
                if(enemyIsNull)
                {
                    meleeAttackBehaviourState = MeleeAttackBehaviourState.Idle;
                    movement.Stop();
                }
                //else check if we reached the destination
                else if(currentSquaredDistanceToNearestEnemy < rangeToEnterMeleeCombatSquared)
                {
                    meleeAttackBehaviourState = MeleeAttackBehaviourState.InMeleeFight;
                }
                //if not, check if we need to update the destination
                else
                {
                    if(Time.time > nextUpdateDestinationTime)
                    {
                        nextUpdateDestinationTime = Time.time + updateDestinationInterval;
                        movement.MoveTo(nearestEnemyPosition);
                    }
                }

                break;

            case MeleeAttackBehaviourState.InMeleeFight:
                
                //check if the enemy is still alive
                if (enemyIsNull)
                {
                    meleeAttackBehaviourState = MeleeAttackBehaviourState.Idle;
                    movement.Stop();
                }
                //check if the enemy has not escaped
                else if (currentSquaredDistanceToNearestEnemy > rangeToEnterMeleeCombatSquared)
                {
                    if (combat.currentCombatState == CombatState.CombatIdle)
                    {
                        movement.MoveTo(nearestEnemyPosition);
                        meleeAttackBehaviourState = MeleeAttackBehaviourState.MovingToNearestEnemy;
                    }
                }
                //else check if i am in range for an attack - fix range and attack 
                else
                {
                    //decision making .- thats the tricky part here
                    if (combat.currentCombatState == CombatState.CombatIdle)
                    {
                        //check if we should block because our adversary was faster
                        if (nearestEnemy.combat.currentCombatState == CombatState.PreparingMeleeAttack)
                        {
                            //if he is lucky he will block or evade, if not then not :D
                            if (Random.Range(0, 2) == 0)
                            {
                                if (Random.Range(0, 2) == 0)
                                {
                                    combat.Block(currentStance.blocks[0]);
                                }
                                else
                                {
                                    combat.Evade(currentStance.evades[0]);
                                }
                            }
                            else
                            {
                                combat.MeleeAttack(nearestEnemy, currentStance.attacks[Random.Range(0, currentStance.attacks.Length)]);
                            }
                        }
                        else
                        {
                            combat.MeleeAttack(nearestEnemy, currentStance.attacks[Random.Range(0, currentStance.attacks.Length)]);
                        }
                    }
                    else if (combat.currentCombatState == CombatState.Blocking)
                    {
                        //check if we can stop blocking and attack
                        if (nearestEnemy.combat.currentCombatState != CombatState.PreparingMeleeAttack)
                        {

                            combat.MeleeAttack(nearestEnemy, currentStance.attacks[Random.Range(0, currentStance.attacks.Length)]);
                        }
                    }
                }
                break;


                
        }

        nearestEnemyLastFrame = nearestEnemy;

       // Profiler.EndSample();
    }
}
