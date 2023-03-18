using Unity.Entities;
using Unity.Mathematics;

namespace DOD.Scripts.Bullets
{
    public struct BulletSpawnerPositionComponent : IComponentData
    {
        // public Entity Prefab;
        public float3 SpawnPosition;
        // public float NextSpawnTime;
        // public float SpawnRate;
    }
}
