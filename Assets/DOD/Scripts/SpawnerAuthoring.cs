using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOD.Scripts
{
    class SpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public GameObject SpawnPositionPrefab;
        public float SpawnPositionX;
        public float SpawnRate;
    }

    class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var transform = GetComponent<Transform>(authoring.SpawnPositionPrefab);
            AddComponent(new Spawner
            {
                // By default, each authoring GameObject turns into an Entity.
                // Given a GameObject (or authoring component), GetEntity looks up the resulting Entity.
                Prefab = GetEntity(authoring.Prefab),
                SpawnPositionX = transform.position.x,
                NextSpawnTime = 0.0f,
                SpawnRate = authoring.SpawnRate
            });
        }
    }
}