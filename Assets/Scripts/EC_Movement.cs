using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

/*
 * one of the basic components which are used by an entity if needed
 * takes care of the navmesh agent implementation
 */

[System.Serializable]
public class EC_Movement : EntityComponent
{
    [HideInInspector]
    public NavMeshAgent agent;
    protected EC_UnitAnimation unitAnimation;

    public int movementState; //0 is walk, 1 is run, 2 is sprint

    //for rotation independent of navmeshAgent;
    float angularSpeed;

    //optimising transform call can be expensive on large scale
    Transform myTransform;



    //for smoothing the animation value - to prevent too fast changes from standing to running
    float smoothRate = 20;
    float animationValueLastTime = 0;

    //for optimisation we can call the updater only every x frames
    float nextMovementUpdateTime;
    public float movementUpdateIntervall = 1 / 6;

    //our agent can either rotate to the direction he is facing or have a target to which he is alwys rotated to - if lookAt is true
    Transform targetToLookAt;
    bool lookAt = false;
    float lastRotationTime; // if we rotate only once every x frames, we need to calculate our own deltaTIme

    public override void SetUpEntityComponent(GameEntity entity)
    {
        base.SetUpEntityComponent(entity);

        agent = entity.GetComponent<NavMeshAgent>();
        unitAnimation = entity.unitAnimation;

        //optimisation
        angularSpeed = agent.angularSpeed;
        myTransform = entity.myTransform;
        nextMovementUpdateTime = Time.time + Random.Range(0, movementUpdateIntervall);

    }

    //update is only for looks- the rotation is important for logic but it can be a bit jaggy if far away or not on screen - lod this script, only call it every x seconds
    public override void UpdateEntityComponent(float deltaTime, float time)
    {
        if (time > nextMovementUpdateTime)
        {
            nextMovementUpdateTime += movementUpdateIntervall;

            //Profiler.BeginSample("MovementUpdateAnim");

            float animationValue = Mathf.Lerp(animationValueLastTime, agent.velocity.magnitude, smoothRate * deltaTime);
            // Debug.Log("actual value: " + agent.velocity.magnitude);
            //Debug.Log("smoothed value: " + animationValue);
            unitAnimation.SetMovementAnimationBlendValue(animationValue, agent.speed);

            animationValueLastTime = animationValue;




            //Profiler.EndSample();
            //Profiler.BeginSample("LookAt");

            if (lookAt)
            {
                if (targetToLookAt != null)
                {
                    RotateTo(targetToLookAt.position - myTransform.position, time);
                }
            }
            //Profiler.EndSample();


        }

        if (!agent.isActiveAndEnabled)
        {
            Debug.Log(agent.isActiveAndEnabled + " : " + myTransform.name);
        }
    }


    //sets the agent to rotate 
    public void RotateTo(Vector3 direction, float time)
    {
        direction.y = 0;
        //Profiler.BeginSample("LookRotation");
        Quaternion desiredLookRotation = Quaternion.LookRotation(direction);
        //Profiler.EndSample();
        //because we want the same speed as the agent, which has its angular speed saved as degrees per second we use the rotaate towards function
        //Profiler.BeginSample("Setotation");
        myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, desiredLookRotation, angularSpeed * time - lastRotationTime);
        lastRotationTime = time;
        //Profiler.EndSample();
    }

    //for now simple moveTo without surface ship or flying
    public void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
    }

    //this method tells the agent to look at a specific target while moving
    public void LookAt(Transform targetToLookAt)
    {
        this.targetToLookAt = targetToLookAt;
        agent.updateRotation = false;
        lookAt = true;
    }

    public void StopLookAt()
    {
        agent.updateRotation = true;
        lookAt = false;
    }

    public bool isMoving()
    {
        return agent.speed > 0;
    }

    public void Stop()
    {
        agent.ResetPath();
    }
}

