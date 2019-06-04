using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * combat actions are used by the combat class - only hold date for the different attacks - how long are they, which range, which weapon is needed, which animation?
 */

[System.Serializable]
public class CombatAction 
{
    [Space(10)]
    public string actionName;

}

[System.Serializable]
public class CA_MeleeAttack : CombatAction
{
    [Header("Stats")]

    public int meleeDamage = 5;
    [Tooltip("approximate of in which distance to the enemy this attack should be launched")]
    public float meleeRange = 2;

    [Header("Timing Stats")]
    public float prepareMeleeAttackTime = 2f; //the time from starting the swing to landing the hit
    public float finishAttackTime = 2f;
    public float attackFlinchTime = 0.5f; // can be changed by some effects?

    [Header("Animation")]
    [Tooltip("this string gets changet into an int at start by the unitAI - to improve performance")]
    public string animatorTriggerString;
    [HideInInspector]
    public int animatorTriggerID;

    [Tooltip("some attacks may have movement accompaning them")]
    [Header("Movement")]
    public Vector3 movementDirection;
    public float movementDistance;
    public bool hasMovement = false;

    //use an sphere object to measure it, if at the end of the attack our hit sphere is colliding with another unit, we deal damage to him
    [Header("HitSphere")]
    [Tooltip("relativeToTheUnit")]
    public Vector3 hitSpherePosition;
    public float hitSphereRadius;

    public void SetUp()
    {
        animatorTriggerID = Animator.StringToHash(animatorTriggerString);
    }

}

[System.Serializable]
public class CA_Block : CombatAction
{
    public float prepareBlockingTime = 0.2f;
    public float blockFlinchTime = 0.2f; // can be changed by some effects?

    [Tooltip("wha´t area does our block cover - 45 means we have a 90 degree cone, our block will block attacks coming from the directions of this cone")]
    public float blockCoveringAngle; //how much space does this block cover? - how big is the blocking wall - only in 2d-  y and z coordinate, we leave the z out

    [Header("Animation")]
    [Tooltip("this string gets changet into an int at start by the unitAI - to improve performance")]
    public string animatorTriggerString;
    [HideInInspector]
    public int animatorTriggerID;

    public void SetUp()
    {
        animatorTriggerID = Animator.StringToHash(animatorTriggerString);
    }

}

[System.Serializable]
public class CA_Evade : CombatAction
{
    [Header("Animation")]
    [Tooltip("this string gets changet into an int at start by the unitAI - to improve performance")]
    public string animatorTriggerString;
    [HideInInspector]
    public int animatorTriggerID;

    public Vector3 direction;
    public float distance;
    public float evadeTime;

    public void SetUp()
    {
        animatorTriggerID = Animator.StringToHash(animatorTriggerString);
    }
}

/* //not sure about heese yet
[System.Serializable]
public class DrawWeapon: CombatAction
{
    public AnimationClip animationClip;

    public float drawingTime;
}

public class HolsterWeapon : CombatAction
{
    public AnimationClip animationClip;

    public float holsterTime;
}*/
