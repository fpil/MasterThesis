using DOD.Scripts.Bullets;
using Unity.Entities;
using UnityEngine;

namespace DOD.Scripts.Bullets
{
    public class BulletSpawnerBaker : MonoBehaviour
    {
        // public GameObject spawnPosition;
        public Transform bulletPrefab; 
    }
}

public struct BulletSpawnPositionComponent : IComponentData
{
    public Entity BulletPrefab;
    // public Entity BulletSpawn;
}

public class BulletBaker : Baker<BulletSpawnerBaker>
{
    public override void Bake(BulletSpawnerBaker authoring)
    {
        AddComponent(new BulletSpawnPositionComponent
        {
            BulletPrefab = GetEntity(authoring.bulletPrefab),
            // BulletSpawn = GetEntity(authoring.spawnPosition),
        } );
    }
}
