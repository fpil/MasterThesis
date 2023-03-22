using Assets.DOD.Scripts.Bullets;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DOD.Scripts.Bullets
{
    [UpdateAfter(typeof(BulletSpawnSystem))]
    public partial struct BulletBehaviourSystem : ISystem
    {
        private SystemHandle buildPhysicsWorldSystem;
        private SystemHandle commandBufferSystem;

        public void OnCreate(ref SystemState state)
        {
            buildPhysicsWorldSystem = state.World.GetOrCreateSystem<BuildPhysicsWorld>(); 
            // commandBufferSystem = state.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer.ParallelWriter ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            // PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            var deltaTime = state.WorldUnmanaged.Time.DeltaTime;

            
            
            
            
            
            
            
            
            // EntityManager.CompleteDependencyBeforeRW<PhysicsWorldSingleton>();
            
            // var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            // var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
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

            // updateBulletPositionJob.Run();
            
            var bulletCollisionJob = new BulletCollisionJob()
            {
                deltaTime = deltaTime,
                // commandBuffer = ecb,
                
                world = collisionWorld
            };    
            //Need to be paralle or race condition
            state.Dependency = bulletCollisionJob.ScheduleParallel(state.Dependency);
            // bulletCollisionJob.Run();
            
            
            
            
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

                    //This is expensive structural change
                    // Ecb.RemoveComponent<BulletFired>(entity);
                    
                }
                //Update position of the bullet
                localTransform.Position += fired.fireDirection * 10 * deltaTime; //todo --> change the speed parameter to be bullet dependent 

            }
        }
        
        [UpdateAfter(typeof(UpdateBulletPositionJob))]
        [WithAll(typeof(BulletTag))]
        public partial struct BulletCollisionJob : IJobEntity
        {
            public float deltaTime;
            // public RaycastInput raycastInput;
            // public EntityCommandBuffer.ParallelWriter commandBuffer;
            [field: ReadOnly] public CollisionWorld world { get; set; }

            public void Execute(in Entity entity, in LocalTransform localTransform, in BulletFired fired)
            {
                
                var v = new Vector3(fired.fireDirection.x,fired.fireDirection.y,fired.fireDirection.z)*1;
                Debug.Log(v );

                var raycastInput = new RaycastInput
                {
                    Start = localTransform.Position,
                    End = localTransform.Position*fired.fireDirection,
                    Filter = CollisionFilter.Default
                };
                
                Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.green, 0.1f);
                
                // var hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);
                RaycastHit hit = new RaycastHit();
                // Debug.Log(world.CastRay(raycastInput, ref hits));

                // var ray = new PhysicsWorld().CastRay(raycastInput, ref hits); 
                
                if (world.CastRay(raycastInput, out hit))
                {
                    // Debug.Log("hit");
                    if (hit.Entity != entity)
                    {
                        Debug.Log(hit.Entity);

                    }
                
                }
                else
                {
                    Debug.Log("Not hit");
                }
                // hits.Dispose();

            }
        }
    }
}
    
    

