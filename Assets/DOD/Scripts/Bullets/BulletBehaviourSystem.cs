using Assets.DOD.Scripts.Bullets;
using DOD.Scripts.Enemies;
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
        // private SystemHandle buildPhysicsWorldSystem;
        private SystemHandle commandBufferSystem;

        public void OnCreate(ref SystemState state)
        {
            // buildPhysicsWorldSystem = state.World.GetOrCreateSystem<BuildPhysicsWorld>(); 
            // commandBufferSystem = state.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = state.WorldUnmanaged.Time.DeltaTime;

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            // PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            var muzzleGameObject = GameObject.Find("MuzzleGameObject"); //this is not a good approach to ref the position like this
            var vector3 = muzzleGameObject.transform.forward;

            var updateBulletPositionJob = new UpdateBulletPositionJob()
            {
                deltaTime = deltaTime,
                vector3 = vector3,
                // Ecb = ecb
            };    
            state.Dependency = updateBulletPositionJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();


            
            var bulletCollisionJob = new BulletCollisionJob()
            {
                world = collisionWorld,
                entityManager = state.World.EntityManager
            };
            bulletCollisionJob.Run(); //Cannot be parallel because the entity manger is used // todo --> fix
            state.Dependency.Complete();
            // bulletCollisionJob.ScheduleParallel(); 
            // state.Dependency = bulletCollisionJob.ScheduleParallel(state.Dependency);
            
            
            var destroyBulletJob = new DestroyBulletJob()
            {
                ECB = ecb.AsParallelWriter(),
            };    
            state.Dependency = destroyBulletJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            
        }

        [WithAll(typeof(BulletTag))]
        public partial struct UpdateBulletPositionJob : IJobEntity
        {
            public float deltaTime;
            public Vector3 vector3 { get; set; }
            // public EntityCommandBuffer Ecb;

            public void Execute(ref LocalTransform localTransform, ref BulletFired fired, ref BulletLifeTime lifeTime)
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
                localTransform.Position += fired.fireDirection * 100 * deltaTime; //todo --> change the speed parameter to be bullet dependent 
                lifeTime.currentLifeTime += deltaTime;

            }
        }
        
        [UpdateAfter(typeof(UpdateBulletPositionJob))]
        [WithAll(typeof(BulletTag))]
        public partial struct BulletCollisionJob : IJobEntity
        {
            // public float deltaTime;
            [field: ReadOnly] public CollisionWorld world { get; set; }
            public EntityManager entityManager;

            public void Execute(in Entity entity, in LocalTransform localTransform, in BulletFired fired, ref BulletLifeTime lifeTime)
            {
                var raycastInput = new RaycastInput
                {
                    Start = localTransform.Position,
                    End = localTransform.Position+fired.fireDirection,//*10*deltaTime,
                    Filter = CollisionFilter.Default
                };
                
                // Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.green, 0.1f);
                
                RaycastHit hit = new RaycastHit();
                if (world.CastRay(raycastInput, out hit))
                {
                    if (hit.Entity != entity)
                    {
                        bool has = entityManager.HasComponent<MeleeEnemyTag>(hit.Entity);
                        if (has)
                        {
                            //todo --> deal damage to enemy
                            lifeTime.currentLifeTime = 2; //This will trigger the bullet to be deleted // todo --> maybe too much a hack
                            Debug.Log(hit.Entity);
                        }
                    }
                }
            }
        }
        
        [UpdateAfter(typeof(UpdateBulletPositionJob))]
        [WithAll(typeof(BulletTag))]
        public partial struct DestroyBulletJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute([ChunkIndexInQuery] int chunkIndex,in Entity entity, in BulletLifeTime lifeTime)
            {
                if (lifeTime.currentLifeTime >= lifeTime.maxLifeTime)
                {
                    ECB.DestroyEntity(chunkIndex,entity); //This is expensive calls 
                }
            }
        }
    }
}
    
    

