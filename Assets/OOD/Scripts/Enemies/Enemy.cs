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

    private void Update()
    {
        Move();
    }
}
