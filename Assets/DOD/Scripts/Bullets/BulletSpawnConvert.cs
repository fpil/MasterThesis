using System.Collections;
using System.Collections.Generic;
using DOD.Scripts;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOD.Scripts.Bullets
{
    public class BulletSpawnConvert : MonoBehaviour
    {
        public GameObject bulletSpawnPosition;
        public GameObject Prefab;
    }
    class BulletSpawnerBaker : Baker<BulletSpawnConvert>
    {
        public override void Bake(BulletSpawnConvert authoring)
        {
            AddComponent(new BulletSpawnerPositionComponent
            {
                // BulletSpawnPosition = new float3(authoring.bulletSpawnPosition.transform),
                Prefab = GetEntity(authoring.Prefab)
            });
        }
    }
}