using Assets.DOD.Scripts.Bullets;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

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
            // var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            // var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            var deltaTime = state.WorldUnmanaged.Time.DeltaTime;
            
            var muzzleGameObject = GameObject.Find("MuzzleGameObject"); //this is not a good approach to ref the position like this
            var vector3 = muzzleGameObject.transform.forward;

            var updateBulletPositionJob = new UpdateBulletPositionJob()
            {
                deltaTime = deltaTime,
                vector3 = vector3,
                // Ecb = ecb
            };    
            //Need to be paralle or race condition
            state.Dependency = updateBulletPositionJob.ScheduleParallel(state.Dependency);

            // bulletFireJob.Run();
        }

        [WithAll(typeof(BulletTag))]
        public partial struct UpdateBulletPositionJob : IJobEntity
        {
            public float deltaTime;
            public Vector3 vector3 { get; set; }
            // public EntityCommandBuffer Ecb;

            public void Execute(ref LocalTransform localTransform, ref BulletFired fired)
            {
                //Saves the original fire direction
                if (fired._hasFired == 0)
                {
                    fired._hasFired = 1;
                    fired.fireDirection = vector3;
                    
                    //Only adds velocity one time
                    // velocity.Linear += new float3(fired.fireDirection * 5000)*deltaTime;

                    //This is expensive structural change
                    // Ecb.RemoveComponent<BulletFired>(entity);
                    
                }
                //Update position of the bullet
                localTransform.Position += fired.fireDirection * 50 * deltaTime; //todo --> change the speed parameter to be bullet dependent 

            }
        }
    }
}
    
    

