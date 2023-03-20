using DOD.Scripts.Bullets;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct BulletSpawnSystem : ISystem
{
    // ComponentLookup<WorldTransform> m_WorldTransformLookup;
    public void OnCreate(ref SystemState state)
    {
        // m_WorldTransformLookup = state.GetComponentLookup<WorldTransform>(true);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        
        // m_WorldTransformLookup.Update(ref state);
        if (Input.GetButtonDown("Fire1"))
        {
            //Used for updating the position of the starting position for the bullet
            var muzzleGameObject = GameObject.Find("MuzzleGameObject");

            foreach (RefRW<BulletSpawnPositionComponent> spawner in SystemAPI.Query<RefRW<BulletSpawnPositionComponent>>())
            {
                BulletSpawner(ref state, spawner, muzzleGameObject);
            }
        }

        
    }
    
    private void BulletSpawner(ref SystemState state, RefRW<BulletSpawnPositionComponent> spawner, GameObject muzzleGameObject)
    {
        // var spawnLocalToWorld = WorldTransformLookup[spawner.ValueRO.BulletSpawn];
        // var bulletTransform = LocalTransform.FromPosition(spawnLocalToWorld.Position);
        var bulletTransform = LocalTransform.FromPosition(muzzleGameObject.transform.position);
        Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.BulletPrefab);
        state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(bulletTransform._Position));
    }
}
