using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOD.Scripts
{
    public struct Spawner : IComponentData
    {
        public Entity Prefab;
        public float SpawnPositionX;
        public float NextSpawnTime;
        public float SpawnRate;
    }
}
