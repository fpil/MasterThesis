using DOD.Scripts.Bullets;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct BulletSystem : ISystem
{
    ComponentLookup<WorldTransform> m_WorldTransformLookup;
    public void OnCreate(ref SystemState state)
    {
        m_WorldTransformLookup = state.GetComponentLookup<WorldTransform>(true);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        m_WorldTransformLookup.Update(ref state);
        if (Input.GetButtonDown("Fire1"))
        {
            foreach (RefRW<BulletSpawnPositionComponent> spawner in SystemAPI.Query<RefRW<BulletSpawnPositionComponent>>())
            {
                BulletSpawner(ref state, spawner, m_WorldTransformLookup);
            }
        }

        
    }
    
    private void BulletSpawner(ref SystemState state, RefRW<BulletSpawnPositionComponent> spawner,
        ComponentLookup<WorldTransform> WorldTransformLookup)
    {
        var spawnLocalToWorld = WorldTransformLookup[spawner.ValueRO.BulletSpawn];
        var bulletTransform = LocalTransform.FromPosition(spawnLocalToWorld.Position);
        Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.BulletPrefab);
        state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(bulletTransform._Position));
    }
}
