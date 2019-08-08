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
    Color normalColor;
    Color damagColor;

    [Tooltip("for now used to change color on all the lods")]
    public SkinnedMeshRenderer[] renders;

    public override void SetUpEntityComponent(GameEntity entity)
    {
        base.SetUpEntityComponent(entity);

        currentHealth = maxHealth;
        myTransform = entity.myTransform;

        normalColor = myTransform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().material.color;
        damagColor = new Color(1 - normalColor.r, 1 - normalColor.g, 1 - normalColor.b);
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
        for (int i = 0; i < renders.Length; i++)
        {
            renders[i].material.color = damagColor;
        }

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
        for (int i = 0; i < renders.Length; i++)
        {
            renders[i].material.color = normalColor;
        }
    }
}
