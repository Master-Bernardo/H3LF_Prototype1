using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The container class which gathers all the basic components, which it wants to use is controlled by an entity AI which switches behaviours
 */

public class GameEntity : MonoBehaviour
{
    public int teamID;

    EntityComponent[] components;

    public EC_UnitAI unitAI;
    public EC_Sensing sensing;
    public EC_Movement movement;
    public EC_Health health;
    public EC_Combat combat;
    public EC_UnitAnimation unitAnimation;
    [HideInInspector]
    public Transform myTransform;
    [HideInInspector]
    public bool markAsDestroyed = false;

    public Squad squad;




    void Start()
    {
        GameEntityManager.Instance.AddGameEntity(this);

        //set the variables

        myTransform = transform;

        //setup all attached components
        components = new EntityComponent[] {sensing, movement, combat, unitAnimation, unitAI, health }; //unit ai needs to go after unitANimation

        for (int i = 0; i < components.Length; i++)
        {
           components[i].SetUpEntityComponent(this);
        }
    }

    public void UpdateGameEntity(float deltaTime, float time)
    {
        for (int i = 0; i < components.Length; i++)
        {
           components[i].UpdateEntityComponent(deltaTime, time);
        }
    }

}
