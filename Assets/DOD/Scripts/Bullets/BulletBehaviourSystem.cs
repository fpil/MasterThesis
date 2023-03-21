using Assets.DOD.Scripts.Bullets;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEditor.SceneManagement;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DOD.Scripts.Bullets
{
    [UpdateAfter(typeof(BulletSpawnSystem))]
    public partial struct BulletBehaviourSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var deltaTime = state.WorldUnmanaged.Time.DeltaTime;
            
            //Todo --> this does not really work because the job is never stalled
            var muzzleGameObject = GameObject.Find("MuzzleGameObject");
            var vector3 = muzzleGameObject.transform.forward;

            var bulletFireJob = new BulletFireJob()
            {
                deltaTime = deltaTime,
                vector3 = vector3,
                Ecb = ecb
            };    
            //Need to be paralle or race condition
            // state.Dependency = bulletFireJob.ScheduleParallel(state.Dependency);

            bulletFireJob.Run();
        }

        [WithAll(typeof(BulletTag))]
        public partial struct BulletFireJob : IJobEntity
        {
            public float deltaTime;
            public Vector3 vector3 { get; set; }
            public EntityCommandBuffer Ecb;

            public void Execute(in Entity entity, ref PhysicsVelocity velocity, ref BulletFired fired)
            {
                //Saves the original fire direction
                if (fired._hasFired == 0)
                {
                    fired._hasFired = 1;
                    fired.fireDirection = vector3;
                    //Only adds velocity one time
                    velocity.Linear += new float3(fired.fireDirection * 5000)*deltaTime;

                    //This is expensive structural change
                    Ecb.RemoveComponent<BulletFired>(entity);
                    
                }
            }
        }
    }
}
    
    

