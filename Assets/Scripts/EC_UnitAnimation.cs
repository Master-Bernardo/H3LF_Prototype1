using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/*
 * Takes care of the animation stuff , scales time scalable animations like the attack and blocking animations 
 */

[System.Serializable]
public class EC_UnitAnimation : EntityComponent
{
    public Animator animator;

    List<int> allTriggerUnitInternIDs = new List<int>(); // for optimising performance, call triggers by thei id´s indstead of their names and reset them by their id´s

    int movementSpeedBlendFloatID;
    int prepareAttackSpeedFloatID;
    int finishAttackSpeedFloatID;
    int attackFlinchSpeedFloatID;
    int prepareBlockSpeedFloatID;
    int blockFlinchSpeedFloatID;
    int damageFlinchSpeedFloatID;

    int backToIdleTriggerID;
    int finishAttackTriggerID;
    int attackFlinchTriggerID;
    int blockFlinchTriggerID;
    int blockTriggerID;
    int damageFlinchTriggerID;
    int currentStanceID;




    //TODO create more id´s for the rest of the parameters

    public void SetUpIDs()
    {
        //get all the params id´s - calling them by their id´s optimises performance
        movementSpeedBlendFloatID = Animator.StringToHash("MovementSpeedBlend");
        prepareAttackSpeedFloatID = Animator.StringToHash("PrepareAttackSpeed");
        finishAttackSpeedFloatID = Animator.StringToHash("FinishAttackSpeed");
        attackFlinchSpeedFloatID = Animator.StringToHash("AttackFlinchSpeed");
        prepareBlockSpeedFloatID = Animator.StringToHash("PrepareBlockSpeed");
        blockFlinchSpeedFloatID = Animator.StringToHash("BlockFlinchSpeed");
        damageFlinchSpeedFloatID = Animator.StringToHash("DamageFlinchSpeed");

        currentStanceID = Animator.StringToHash("CurrentStance");

        backToIdleTriggerID = Animator.StringToHash("BackToIdle");
        finishAttackTriggerID = Animator.StringToHash("FinishAttack");
        attackFlinchTriggerID = Animator.StringToHash("AttackFlinch");
        blockFlinchTriggerID = Animator.StringToHash("BlockFlinch");
        blockTriggerID = Animator.StringToHash("Block");
        damageFlinchTriggerID = Animator.StringToHash("DamageFlinch");
    }

    //at the set up we get the ids from all the triggers for better performance
    public override void SetUpEntityComponent(GameEntity entity)
    {
        base.SetUpEntityComponent(entity);

        SetUpIDs();

        AnimatorControllerParameter param;

        for (int i = 0; i < animator.parameters.Length; i++)
        {
            param = animator.parameters[i];

            if(param.type == AnimatorControllerParameterType.Trigger)
            {
                allTriggerUnitInternIDs.Add(param.nameHash);
            }
        }

        

    }

    //necessary for animation change
    public void ChangeStance(int newStance)
    {
        animator.SetInteger(currentStanceID, newStance);
    }




    //gets called by movement
    public void SetMovementAnimationBlendValue(float currentSpeed, float maxSpeed)
    {
        Profiler.BeginSample("AnimationMovementFloat");
        //Debug.Log("currentSpeed: " + currentSpeed);
        //Debug.Log("maxSpeed" + maxSpeed);
        animator.SetFloat(movementSpeedBlendFloatID, (currentSpeed / maxSpeed));
        Profiler.EndSample();
    }


    //swingTime is the time from the beginning of the animation to the landing of the hit, the animation should scale itself to this time - duration is in seconds
    //id represents which attack animations we should play - some attacks can have several animation with the same effect
    public void PlayPrepareAttackAnimation(float duration, int attackID)
    {
        ResetAllTriggers();
        animator.SetFloat(prepareAttackSpeedFloatID, 1/duration);
        animator.SetTrigger(attackID);
    }

    public void BackToIdle()
    {
        ResetAllTriggers();
        animator.SetTrigger(backToIdleTriggerID);
    }

    public void PlayFinishAttackAnimation(float duration)
    {
        ResetAllTriggers();
        animator.SetFloat(finishAttackSpeedFloatID, 1 / duration);
        animator.SetTrigger(finishAttackTriggerID);
    }

    public void PlayAttackFlinchAnimation(float duration)
    {
        ResetAllTriggers();
        animator.SetFloat(attackFlinchSpeedFloatID, 1 / duration);
        animator.SetTrigger(attackFlinchTriggerID);
    }

    public void PlayDamageFlinchAnimation(float duration)
    {
        ResetAllTriggers();
        animator.SetFloat(damageFlinchSpeedFloatID, 1 / duration);
        animator.SetTrigger(damageFlinchTriggerID);
    }

    public void PlayPrepareBlockAnimation(float duration, int blockID)
    {
        ResetAllTriggers();
        animator.SetFloat(prepareBlockSpeedFloatID, 1 / duration);

        animator.SetTrigger(blockID);
    }

    public void PlayBlockAnimation()
    {
        ResetAllTriggers();
        animator.SetTrigger(blockTriggerID);
    }

    public void PlayBlockFlinchAnimation(float duration)
    {
        ResetAllTriggers();
        animator.SetFloat(blockFlinchSpeedFloatID, 1 / duration);
        animator.SetTrigger(blockFlinchTriggerID);
    }


    //a workaround because strange bugs occur when a trigger is called while an animation is transitioning - to solve it we either reset all triggers before setting new ones or take transitions out
    void ResetAllTriggers()
    {
        for (int i = 0; i < allTriggerUnitInternIDs.Count; i++)
        {
            animator.ResetTrigger(allTriggerUnitInternIDs[i]);
        }
    }

}