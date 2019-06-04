using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Describes how a unit is standing , has an AnimationOverrideObject attached 
 */

[System.Serializable]
public class UnitStance 
{
    [Space(10)]
    public string name;


    [Header("Actions")]
    public CA_MeleeAttack[] attacks;
    public CA_Block[] blocks;
    public CA_Evade[] evades;
    
}
