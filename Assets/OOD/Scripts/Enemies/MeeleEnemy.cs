using System;
using UnityEngine;

namespace OOD.Scripts.Enemies
{
    public class MeeleEnemy : Enemy
    {
        public int attackDamage;
        private Transform player;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        public override void Move() {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        public override void Attack() {
            // Debug.Log("Attack");
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag == "Player")
            {
                Attack();
            }
            if (other.gameObject.tag == "Bullet1")
            {
                //Decrease health of enemy
                health -= 5; //todo --> change the value to be bullet specific 

                //Destroy the bullet on impact with enemy
                Destroy(other.gameObject);
            }
        }
    }
}
