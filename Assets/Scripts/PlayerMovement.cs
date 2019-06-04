using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//almost like the normal movement class, with the difference that the navmesh agent does not rotae at all, while in the normal movement in roatetes except when in combat
public class PlayerMovement : EC_Movement
{
    public bool posessedByPlayer;

    Vector3 currentDesiredLookDirection;

    public override void SetUpEntityComponent(GameEntity entity)
    {
        base.SetUpEntityComponent(entity);
        if(posessedByPlayer) agent.updateRotation = false; //we update the rotation by ourselves
        currentDesiredLookDirection = entity.transform.forward;
    }

    //maybe only for player character?
    public override void UpdateEntityComponent(float deltaTime, float time)
    {
        base.UpdateEntityComponent(deltaTime, time);
        if (posessedByPlayer)
        {
            if (agent.desiredVelocity.sqrMagnitude > 0) currentDesiredLookDirection = agent.desiredVelocity;
            RotateTo(currentDesiredLookDirection, deltaTime);
        } 
    }

    public void Posess()
    {
        posessedByPlayer = true;
        agent.updateRotation = false;
    }

    public void Deposess()
    {
        posessedByPlayer = false;
        agent.updateRotation = true;
    }
}

