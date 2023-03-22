using Assets.DOD.Scripts.Bullets;
using DOD.Scripts.Bullets;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct BulletSpawnSystem : ISystem
{
    // ComponentLookup<WorldTransform> m_WorldTransformLookup;
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
        // Creating an EntityCommandBuffer to defer the structural changes required by instantiation.
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        
        if (Input.GetButtonDown("Fire1"))
        {
            //Used for updating the position of the starting position for the bullet
            var muzzleGameObject = GameObject.Find("MuzzleGameObject").transform.position;

            var bulletSpawnJob = new SpawnBulletJob
            {
                // WorldTransformLookup = m_WorldTransformLookup,
                Ecb = ecb,
                muzzleGameObject = muzzleGameObject
            };

            // Schedule execution in a single thread, and do not block main thread.
            bulletSpawnJob.Run();
            // state.Dependency = bulletSpawnJob.ScheduleParallel(state.Dependency);

            // foreach (RefRW<BulletSpawnPositionComponent> spawner in SystemAPI.Query<RefRW<BulletSpawnPositionComponent>>())
            // {
            //     BulletSpawner(ref state, spawner, muzzleGameObject);
            // }
        }

        
    }
    
    private void BulletSpawner(ref SystemState state, RefRW<BulletSpawnPositionComponent> spawner, GameObject muzzleGameObject)
    {
        var bulletTransform = LocalTransform.FromPosition(muzzleGameObject.transform.position);
        Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.BulletPrefab);
        state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(bulletTransform._Position));
    }

    public partial struct SpawnBulletJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;

        public Vector3 muzzleGameObject { get; set; }
        // public ComponentLookup<WorldTransform> WorldTransformLookup { get; set; }

        void Execute(in BulletSpawnAspect bulletSpawnAspect)
        {
            var instance = Ecb.Instantiate(bulletSpawnAspect.BulletPrefab);
            var cannonBallTransform = LocalTransform.FromPosition(muzzleGameObject);
            Ecb.SetComponent(instance, cannonBallTransform);
            Ecb.SetComponent(instance, new BulletFired
            {
                _hasFired = 0
            });
        }
    }
}
