using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squad : MonoBehaviour
{
    public float squadSensingInterval = 0.5f;
    float nextSquadSensingTime;
    public float squadSensingRadius;

    public int teamID;

    Collider[] enemyCollidersInRange;
    HashSet<Transform> enemyTransformsInRange = new HashSet<Transform>();
    public List<Transform> enemies;

    public bool hierarchicalSensingEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        nextSquadSensingTime = Time.time + Random.Range(0, squadSensingInterval);

        foreach (Transform child in transform)
        {
            child.GetComponent<GameEntity>().squad = this;
            if(hierarchicalSensingEnabled) child.GetComponent<GameEntity>().sensing.hierarchicalSquadSensing = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Time.time> nextSquadSensingTime)
        {
            nextSquadSensingTime += squadSensingInterval;

            enemyTransformsInRange.Clear();


            int layermask = 0;

            if(teamID == 0)
            {
                layermask = 1 << 13;
            }
            else if(teamID == 1)
            {
                layermask = 1 << 12;
            }

            enemyCollidersInRange = Physics.OverlapSphere(transform.position, squadSensingRadius, layermask);

            for (int i = 0; i < enemyCollidersInRange.Length; i++)
            {
                enemyTransformsInRange.Add(enemyCollidersInRange[i].GetComponent<Transform>());
                
            }
            //Debug.Log(enemyTransformsInRange.Count);

            //order them so the units can perform a kind of heuristic which is nearest
            enemies = new List<Transform>(enemyTransformsInRange);

            var comparer = new TransformComparer { myTransform = transform };
            enemies.Sort(comparer);

        }
    }

}

class TransformComparer : IComparer<Transform>
{
    public Transform myTransform;
    public int Compare(Transform x, Transform y)
    {
        return (x.position - myTransform.position).sqrMagnitude.CompareTo((y.position - myTransform.position).sqrMagnitude);
    }
}
