using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OOD.Scripts.Weapon
{
    public class GunController : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public Transform muzzleTransform; 
   
        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            GameObject bullet = Instantiate(bulletPrefab, muzzleTransform.position, muzzleTransform.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(muzzleTransform.forward * 2000f);
            
            //Destroy bullet again
            StartCoroutine(DestroyBulletAfterDelay(bullet, 2));
        }
        IEnumerator DestroyBulletAfterDelay(GameObject bullet, float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(bullet);
        }
    }
}
