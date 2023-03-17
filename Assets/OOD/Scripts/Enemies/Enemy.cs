using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    public float speed;

    public virtual void Move()
    {
    }
    public virtual void Attack()
    {
    }
    public virtual void Die()
    {
        //Destroy gameobject when the enemy has no more health
        Destroy(gameObject);
    }

    private void Update()
    {
        Move();
        if (health <= 0)
        {
            Die();
        }
    }
}
