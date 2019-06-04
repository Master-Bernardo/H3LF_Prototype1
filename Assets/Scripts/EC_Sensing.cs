using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/*
 * one of the basic components which are used by an entity if needed
 * scans the world around it for other entities or other things, if need be
 * 
 * enery x seconds we update the nearest enemy the nearest friendlies and the enemies and friendlies in range collections, so the ai can check them
 */

[System.Serializable]
public class EC_Sensing : EntityComponent
{
    //Scanning World
    float nextScanWorldTime;
    public float scanWorldInterval;
    [Tooltip("only used if hierarchicalSensing is off")]
    public float scanWorldRadius;

    //Variables we set while scanning
    List<Transform> surroundingEnemiesTransformsList = new List<Transform>(); //this one is used if the squad gives us the enemies - if hierarchicalSensing is on - there we need a sorted list

   // HashSet<GameEntity> surroundingFriendlies = new HashSet<GameEntity>();
    //HashSet<Transform> surroundingEnemiesTransformsSet = new HashSet<Transform>(); 
    //do we need them?

    Transform nearestEnemyTransform; 
    [HideInInspector]
    public GameEntity nearestEnemy;

    Squad squad;


    public bool showScanWorldRadius = false;

    Transform myTransform;

    [Tooltip("if true, the seinsing will be herarchical through a squad - requires the unit to be in a squad")]
    public bool hierarchicalSquadSensing;


    public override void SetUpEntityComponent(GameEntity entity)
    {
        base.SetUpEntityComponent(entity);

        myTransform = entity.myTransform;
        nextScanWorldTime = Time.time + Random.Range(0, scanWorldInterval);
        squad = entity.squad;
    }

    //should not be lodded, is already lodded, optimise searching algorthm - hierarchical search using squads and distance check
    public override void UpdateEntityComponent(float deltaTime, float time)
    {
        Profiler.BeginSample("Sensing");
        if (time > nextScanWorldTime)
        {
            nextScanWorldTime += scanWorldInterval;

            ScanSurroundingUnits();
        }
        Profiler.EndSample();
    }

    void ScanSurroundingUnits()
    {

        //if we use normal sensing , the unit itself will perform a sphere overlap and search the resulting collection for the nearest
        if (!hierarchicalSquadSensing)
        {
            Profiler.BeginSample("SensingUpdateOverlapSphereNotHierarchical");
            //one way, if there are not too many units
            int layerMask = 0;

            if (entity.teamID == 0)
            {
                 layerMask = 1 << 13;
            }
            else if (entity.teamID == 1)
            {
                 layerMask = 1 << 12;
            }
       
            Collider[] nearestUnits = Physics.OverlapSphere(myTransform.position, scanWorldRadius, layerMask);

            Vector3 myPosition = myTransform.position;
            float currentDistance = 0;
            Collider nearestEnemyCol = null;
            float nearestDistance = Mathf.Infinity;

            foreach (Collider unitCollider in nearestUnits)
            {
                currentDistance = (unitCollider.transform.position - myPosition).sqrMagnitude;
                if (currentDistance < nearestDistance)
                {
                    nearestDistance = currentDistance;
                    nearestEnemyCol = unitCollider;
                }
            }

            if (nearestEnemyCol != null) nearestEnemy = nearestEnemyCol.GetComponent<GameEntity>();

            Profiler.EndSample();
        }
        else
        {
            //if hierarchical sensing is on, we get the colleciton of nearestUnits from the squad, which poerformas a sphere overlap and traverse this for the nearest -> better performance

            Profiler.BeginSample("SensingUpdateHierarchical");

            surroundingEnemiesTransformsList = entity.squad.enemies;

            //get the nearestEnemy
            nearestEnemy = null;
            float nearestDistance = Mathf.Infinity;

            Vector3 myPosition = myTransform.position;
            float currentDistance = 0;

            //lets try some heuristic - the squads sort their lists by distance to the squads middle position, so we traverse the list -
            //and stop if we havnt found a closer unit after traversing 20 more eleemtns

            int traverseTreshold = 20;
            int currentSkippedElements = 0; //wieviele Elemente haben wir übersprungen, welche nicht näher waren

            foreach (Transform enemyTransform in surroundingEnemiesTransformsList)
            {
                if (enemyTransform != null)
                {
                    currentDistance = (enemyTransform.position - myPosition).sqrMagnitude;
                    if (currentDistance < nearestDistance)
                    {
                        nearestDistance = currentDistance;
                        nearestEnemyTransform = enemyTransform;

                        currentSkippedElements = 0;
                    }
                    else
                    {
                        currentSkippedElements++;
                    }
                }

                if (currentSkippedElements == traverseTreshold)
                {
                    break;
                }
            }

            if (nearestEnemyTransform != null) nearestEnemy = nearestEnemyTransform.GetComponent<GameEntity>();

            Profiler.EndSample();
        }
        


       


    }

    private void OnDrawGizmos()
    {
        if (showScanWorldRadius)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(myTransform.position, scanWorldRadius);
        }
    }
}
