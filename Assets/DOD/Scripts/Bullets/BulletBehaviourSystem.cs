using Assets.DOD.Scripts.Bullets;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DOD.Scripts.Bullets
{
    [UpdateAfter(typeof(BulletSpawnSystem))]
    [BurstCompile]
    public partial struct BulletBehaviourSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = state.WorldUnmanaged.Time.DeltaTime;
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            var updateBulletPositionJob = new UpdateBulletPositionJob
            {
                deltaTime = deltaTime
            };    
            state.Dependency = updateBulletPositionJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            var bulletCollisionJob = new BulletCollisionJob
            {
                world = collisionWorld,
                Enemies = SystemAPI.GetComponentLookup<EnemyTag>(true),
                Healths = SystemAPI.GetComponentLookup<HealthComponent>(true),
                Bullets = SystemAPI.GetComponentLookup<BulletTag>(true),
                ECB = ecb.AsParallelWriter()
            };
            state.Dependency = bulletCollisionJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            
            var destroyBulletJob = new DestroyBulletJob
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

            public void Execute([ChunkIndexInQuery] int chunkIndex, in Entity entity, ref LocalTransform localTransform, ref BulletFired fired, ref BulletLifeTime lifeTime, in SpeedComponent speedComponent)
            {
                //Saves the original fire direction
                if (fired._hasFired == 0)
                {
                    fired._hasFired = 1;
                    fired.fireDirection = localTransform.Forward();
                }
                //Update position of the bullet
                localTransform.Position += fired.fireDirection * speedComponent.Value * deltaTime; 
                lifeTime.currentLifeTime += deltaTime;
                
                // if (lifeTime.currentLifeTime >= lifeTime.maxLifeTime)
                // {
                //     ECB.SetComponentEnabled<IsDeadComponent>(chunkIndex, entity,true);
                // }
            }
        }
        
        [UpdateAfter(typeof(UpdateBulletPositionJob))]
        [WithAll(typeof(BulletTag))]
        [BurstCompile]
        public partial struct BulletCollisionJob : IJobEntity
        {
            [field: ReadOnly] public CollisionWorld world { get; set; }
            [ReadOnly]
            public ComponentLookup<HealthComponent> Healths;
            [ReadOnly]
            public ComponentLookup<EnemyTag> Enemies;
            [ReadOnly]
            public ComponentLookup<BulletTag> Bullets;
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute([ChunkIndexInQuery] int chunkIndex, in Entity entity, in LocalTransform localTransform, in BulletFired fired, ref BulletLifeTime lifeTime)
            {
                var rayCastInput = new RaycastInput
                {
                    Start = localTransform.Position,
                    End = localTransform.Position+fired.fireDirection,
                    Filter = CollisionFilter.Default
                };
                
                // Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.green, 0.1f);
                RaycastHit hit = new RaycastHit();
                if (world.CastRay(rayCastInput, out hit))
                {
                    if (hit.Entity != entity && !Bullets.HasComponent(hit.Entity))
                    {
                        if (Enemies.HasComponent(hit.Entity))
                        {
                            var currentHealth = Healths.GetRefRO(hit.Entity).ValueRO;
                            if (currentHealth.value-5 <=0) //Small trick to get the correct value after hit without ref again
                            {
                                ECB.SetComponentEnabled<IsDeadComponent>(hit.Entity.Index, hit.Entity,true);
                            }
                            else
                            {
                                ECB.SetComponent(hit.Entity.Index, hit.Entity, new HealthComponent
                                {
                                    value = currentHealth.value -= 5
                                } );
                            }
                        }
                        //Destroy bullet if it collides with something
                        lifeTime.currentLifeTime = 2; // todo --> maybe too much a hack
                        // ECB.SetComponentEnabled<IsDeadComponent>(chunkIndex, entity,true);
                    }
                }
            }
        }
        
        [UpdateAfter(typeof(UpdateBulletPositionJob))]
        [WithAll(typeof(BulletTag))]
        [BurstCompile]
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
    
    

