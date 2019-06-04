using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 * one of the basic components which are used by an entity if needed
 * counts the health 
 */

[System.Serializable]
public class EC_Health : EntityComponent
{
    public bool alive = true;
    public int maxHealth;
    public int currentHealth;

    public UnityEvent OnDie; //fnction assigned here gets called on die
    public UnityEvent OnTakeDamage; //fnction assigned here gets called on die

    Transform myTransform;
    bool getBackToNormalColor = false;
    float timeToGetBackToNormalColor;

    public override void SetUpEntityComponent(GameEntity entity)
    {
        base.SetUpEntityComponent(entity);

        currentHealth = maxHealth;
        myTransform = entity.myTransform;
    }

    public override void UpdateEntityComponent(float deltaTime, float time)
    {
        if (getBackToNormalColor)
        {
            if (time >= timeToGetBackToNormalColor)
            {
                getBackToNormalColor = false;
                GetBackToNormalColor();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        //Debug.Log("Tekn damage: " + damage);
        if (alive)
        {
            currentHealth -= damage;
            if (currentHealth <= 0) Die();
        }

        OnTakeDamage.Invoke();

        //temporyli we change some colors
        Color currentColor = myTransform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().material.color;

        myTransform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().material.color = new Color(1 - currentColor.r, 1 - currentColor.g, 1 - currentColor.b);

        getBackToNormalColor = true;
        timeToGetBackToNormalColor = Time.time + 0.3f;
    }

    void Die()
    {
        
        alive = false;
        OnDie.Invoke();
        entity.markAsDestroyed = true;
    }

    void GetBackToNormalColor()
    {
        Color currentColor = myTransform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().material.color;

        myTransform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().material.color = new Color(1 - currentColor.r, 1 - currentColor.g, 1 - currentColor.b);
    }
}
