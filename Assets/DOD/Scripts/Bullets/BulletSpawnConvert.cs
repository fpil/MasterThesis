using System.Collections;
using System.Collections.Generic;
using DOD.Scripts;
using Unity.Entities;
using UnityEngine;

namespace DOD.Scripts.Bullets
{
    public class BulletSpawnConvert : MonoBehaviour
    {
        public GameObject bulletSpawnPosition;
    }
    class BulletSpawnerBaker : Baker<BulletSpawnConvert>
    {
        public override void Bake(BulletSpawnConvert authoring)
        {
            AddComponent(new BulletSpawnerPositionComponent
            {
                SpawnPosition = authoring.transform.position
            });
        }
    }
}