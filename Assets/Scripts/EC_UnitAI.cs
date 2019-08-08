using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

/* 
 * has a set of predefined behaviours - switches between them //TODO
 */

[System.Serializable]
public class EC_UnitAI : EntityComponent
{
    public bool posessedByPlayer; //if this is true we controll the unuit, if not an UnitAi controller controlls it

    //check if we need to change bahaviour
    float nextBehaviourChangeCheckeTime;
    public float behaviourChangeCheckInterval;

    //every unit has a current behaviour and behavioursToChooseFrom
    FleeBehaviour fleeBehaviour;
    MeleeAttackBehaviour meleeAttackBehaviour;

    public UnitBehaviour currentBehaviour;

    //some stances are only possible with certain weapons - in the game they are also possible with orhers but will look strange, so change the right weapon for the right stance
    [HideInInspector]
    public UnitStance currentStance;
    public UnitStance[] stances;


    //only update the bahaviour every few seconds?
    float nextBehaviourUpdateTime;
    public float behaviourUpdateInterval;

    Transform myTransform;

    public void SetUpCombatActionsAnimationHashes()
    {
        //we set the up, by calculating their hashValues
        for (int i = 0; i < stances.Length; i++)
        {
            for (int j = 0; j < stances[i].attacks.Length; j++)
            {
                stances[i].attacks[j].SetUp();
            }
            for (int j = 0; j < stances[i].blocks.Length; j++)
            {
                stances[i].blocks[j].SetUp();
            }
            for (int j = 0; j < stances[i].evades.Length; j++)
            {
                stances[i].evades[j].SetUp();
            }
        }
    }


    public override void SetUpEntityComponent(GameEntity entity)
    {
        base.SetUpEntityComponent(entity);

        SetUpCombatActionsAnimationHashes();

        myTransform = entity.myTransform;

        //Time.timeScale = 0.25f; //if we with for slow mo
        ChangeStance(1);

        nextBehaviourChangeCheckeTime = Time.time + Random.Range(0, behaviourChangeCheckInterval);
        nextBehaviourUpdateTime = Time.time + Random.Range(0, behaviourUpdateInterval);

        //construct the bahaviours, assign the starting behaviour
        fleeBehaviour = new FleeBehaviour(entity, myTransform);
        meleeAttackBehaviour = new MeleeAttackBehaviour(entity, myTransform);
        currentBehaviour = meleeAttackBehaviour;
    }

    public override void UpdateEntityComponent(float deltaTime, float time)
    {
       // Profiler.BeginSample("UnitAI");
        //UpdateBehaviour(); later on we want to chekc if we would like to change our current bahaviour before acting according to him

        if (!posessedByPlayer)
        {
            UpdateBehaviour(deltaTime, time);
        }

        //only for testing the state changes
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeStance(1);
        }

        /*if (Input.GetKeyDown(KeyCode.F))
        {
            currentBehaviour = fleeBehaviour;
        }*/
        if (Input.GetKeyDown(KeyCode.M))
        {
            currentBehaviour = meleeAttackBehaviour;
        }
        //Profiler.EndSample();
        //Debug.Log((currentBehaviour as MeleeAttackBehaviour).meleeAttackBehaviourState);
    }

    //checks if we need to change the behaviours
    protected virtual void UpdateBehaviour(float deltaTime, float time)
    {
        
        if (time > nextBehaviourChangeCheckeTime)
        {
            nextBehaviourChangeCheckeTime += behaviourChangeCheckInterval;

            //for now for testing
            /*if (health.currentHealth > 10)
            {
                currentBehaviour = meleeAttackBehaviour;
            }
            else
            {
                currentBehaviour = fleeBehaviour;
            }*/
        }

        if(time> nextBehaviourUpdateTime)
        {
            nextBehaviourUpdateTime += behaviourUpdateInterval;

            currentBehaviour.BehaviourUpdate();
        }
    }

    public void ChangeStance(int newStanceID)
    {
        currentStance = stances[newStanceID];

        entity.unitAnimation.ChangeStance(newStanceID);
        entity.unitAnimation.BackToIdle();
    }


    public void OnEnemySwingsWeaponOnMe()
    {
        currentBehaviour.OnEnemySwingsWeaponAtMe();
    }

    public void OnEnemyWasBlockedByMe()
    {
        currentBehaviour.OnEnemyWasBlockedByMe(); 
    }

}
