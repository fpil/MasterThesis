using DOD.Scripts.Bullets;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct BulletSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        if (Input.GetButtonDown("Fire1"))
        {
            //Used for updating the position of the starting position for the bullet
            var muzzleGameObject = GameObject.Find("MuzzleGameObject").transform.position;

            var bulletSpawnJob = new SpawnBulletJob
            {
                Ecb = ecb,
                muzzleGameObject = muzzleGameObject
            };

            // Schedule execution in a single thread, and do not block main thread.
            bulletSpawnJob.Run();
        }
    }

    [BurstCompile]
    public partial struct SpawnBulletJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public Vector3 muzzleGameObject { get; set; }
        void Execute(in BulletSpawnAspect bulletSpawnAspect)
        {
            var instance = Ecb.Instantiate(bulletSpawnAspect.BulletPrefab);
            var cannonBallTransform = LocalTransform.FromPosition(muzzleGameObject);
            Ecb.SetComponent(instance, cannonBallTransform);
            Ecb.SetComponent(instance, new BulletFired
            {
                _hasFired = 0
            });
            Ecb.SetComponent(instance, new BulletLifeTime
            {
                maxLifeTime = 2
            });
            Ecb.AddComponent(instance, new SpeedComponent
            {
                Value = 10
            });
        }
    }
}
