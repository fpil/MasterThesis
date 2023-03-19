using DOD.Scripts.Bullets;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct BulletSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<BulletSpawnerPositionComponent> spawner in SystemAPI.Query<RefRW<BulletSpawnerPositionComponent>>())
        {
            BulletSpawner(ref state, spawner);
        }
    }
    
    private void BulletSpawner(ref SystemState state, RefRW<BulletSpawnerPositionComponent> spawner)
    {
        // Spawns a new entity and positions it at the spawner.
            Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
            state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(new float3(0,0,0)));
        
            // Resets the next spawn time.
            // spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
    }
}
