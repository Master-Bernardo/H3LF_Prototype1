using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool unitIsPosessedByPlayer; //if this is true we controll the unuit, if not an UnitAi controller controlls it

    public GameEntity playerEntity;
    public GameObject cam;

    EC_Health health;
    EC_Movement movement;
    EC_Combat combat;
    EC_UnitAnimation unitAnimation;
    EC_UnitAI unitAI;

    void Start()
    {
        if (playerEntity != null)
        {
            health = playerEntity.GetComponent<EC_Health>();
            movement = playerEntity.GetComponent<EC_Movement>();
            combat = playerEntity.GetComponent<EC_Combat>();
            unitAnimation = playerEntity.GetComponent<EC_UnitAnimation>();
            unitAI = playerEntity.GetComponent<EC_UnitAI>();
        }

    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (unitIsPosessedByPlayer)
        {
            if (movement.agent.desiredVelocity.sqrMagnitude > 0) movement.RotateTo(movement.agent.desiredVelocity, Time.deltaTime);

            #region movement
           

            if (horizontalInput != 0 || verticalInput != 0)
            {

                Vector3 flatUpVectorOfCamera = Camera.main.transform.up;
                flatUpVectorOfCamera.y = 0;
                flatUpVectorOfCamera.Normalize();
                Vector3 flatRightVectorOfCamera = Camera.main.transform.right;
                flatRightVectorOfCamera.y = 0;
                flatRightVectorOfCamera.Normalize();

                flatRightVectorOfCamera *= horizontalInput;
                flatUpVectorOfCamera *= verticalInput;

                Vector3 movementVector = flatRightVectorOfCamera + flatUpVectorOfCamera;

                if(combat.currentCombatState == CombatState.CombatIdle) movement.MoveTo(playerEntity.transform.position + movementVector);
            }
            else
            {
               // movement.Stop();
            }

            #endregion

            #region combat

            if (Input.GetMouseButtonDown(1))
            {
                combat.Block(unitAI.currentStance.blocks[0]);
            }
            else if (!Input.GetMouseButton(1))
            {
                if (Input.GetKeyDown(KeyCode.G))
                {
                    combat.MeleeAttack(null, unitAI.currentStance.attacks[1]);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (combat.currentCombatState == CombatState.Blocking || combat.currentCombatState == CombatState.CombatIdle || combat.currentCombatState == CombatState.PreparingBlock)
                    {
                        Debug.Log("hor: " + horizontalInput);
                        Debug.Log("vert: " + verticalInput);
                        if (horizontalInput != 0 || verticalInput != 0)
                        {
                            Debug.Log("yes");
                            combat.MeleeAttack(null, unitAI.currentStance.attacks[1]);
                        }
                        else
                        {
                            combat.MeleeAttack(null, unitAI.currentStance.attacks[0]);
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    combat.MeleeAttack(null, unitAI.currentStance.attacks[2]);
                }
                else if (combat.currentCombatState == CombatState.Blocking || combat.currentCombatState == CombatState.PreparingBlock)
                {
                    combat.StopBlocking();
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(horizontalInput != 0 || verticalInput != 0)
                {
                    Debug.Log("yep");
                    Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput);
                    combat.Evade(unitAI.currentStance.evades[0]);
                }
            }

            #endregion
        }
        else
        {
            //simple rts camera movement

            Vector3 flatUpVectorOfCamera = Camera.main.transform.up;
            flatUpVectorOfCamera.y = 0;
            flatUpVectorOfCamera.Normalize();
            Vector3 flatRightVectorOfCamera = Camera.main.transform.right;
            flatRightVectorOfCamera.y = 0;
            flatRightVectorOfCamera.Normalize();

            flatRightVectorOfCamera *= horizontalInput;
            flatUpVectorOfCamera *= verticalInput;

            Vector3 movementVector = flatRightVectorOfCamera + flatUpVectorOfCamera;

            cam.transform.position += movementVector;


        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cam.GetComponent<Camera>().orthographicSize -= Input.GetAxis("Mouse ScrollWheel")*2;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (unitIsPosessedByPlayer) Deposess();
            else Posess();
        }
    }

    void Posess()
    {
        unitIsPosessedByPlayer = true;
        unitAI.posessedByPlayer = true;
        movement.StopLookAt();
        cam.GetComponent<SmoothCameraFollow>().enabled = true;
        // movement.Posess();
    }

    void Deposess()
    {
        unitIsPosessedByPlayer = false;
        unitAI.posessedByPlayer = false;
        cam.GetComponent<SmoothCameraFollow>().enabled = false;
        //movement.Deposess();
    }

    public void OnDieTest()
    {
       // combat.OnDie();
        Destroy(gameObject);
    }

    //TODO applay sensing here
    /*Entity GetUnitInFrontOfMe()
    {
        List<Entity> surroundingEnemies = new List<Entity>();

        int layerMask = 1 << 12;
        float radiusForScan = 2;

        Collider[] nearestUnits = Physics.OverlapSphere(transform.position + transform.forward, radiusForScan, layerMask);


        for (int i = 0; i < nearestUnits.Length; i++)
        {
            Entity thisCombatComponent = nearestUnits[i].GetComponent<Entity>();
           // if (thisCombatComponent.teamID != combat.teamID)
           // {
           //     surroundingEnemies.Add(thisCombatComponent);
           // }
        }

        //get the nearestEnemy
        Entity nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < surroundingEnemies.Count; i++)
        {
            float currentDistance = (surroundingEnemies[i].transform.position - transform.position).sqrMagnitude;
            if (currentDistance < nearestDistance)
            {
                nearestDistance = currentDistance;
                nearestEnemy = surroundingEnemies[i];
            }

        }

        return nearestEnemy;
    }*/
}
