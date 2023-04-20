using DOD.Scripts.Bullets;
using DOD.Scripts.Enemies;
using DOD.Scripts.Environment;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[BurstCompile]
public partial struct EnemyBehaviorSystem : ISystem
{
    private EntityQuery playerQuery;
    public void OnCreate(ref SystemState state)
    {
        playerQuery = state.GetEntityQuery(ComponentType.ReadOnly<PlayerTagComponent>());
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity playerEntity = playerQuery.GetSingletonEntity();
        LocalTransform playerTransform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);
        var deltaTime = state.WorldUnmanaged.Time.DeltaTime;
        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var moveTowardPlayerJob = new MoveAndSeparateEnemiesJob
        {
            PlayerTransform = playerTransform,
            deltaTime = deltaTime,
            world = collisionWorld,
            EntityPositions = SystemAPI.GetComponentLookup<LocalToWorld>(true), //Possibly Expensive
            Enemies = SystemAPI.GetComponentLookup<EnemyTag>(true), //Possibly Expensive
            Obstacles = SystemAPI.GetComponentLookup<ObstacleTag>(true)
        };
        state.Dependency = moveTowardPlayerJob.ScheduleParallel(state.Dependency);

        var destroyEnemyJob = new DestroyEnemyJob
        {
            ECB = ecb.AsParallelWriter()
        };
        state.Dependency = destroyEnemyJob.ScheduleParallel(state.Dependency);
        
        var meleeAttackJob = new MeleeAttackJob
        {
            PlayerTransform = playerTransform, 
            deltaTime = deltaTime
        };
        state.Dependency = meleeAttackJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
        
        var rangeAttackJob = new RangeAttackJob
        {
            PlayerTransform = playerTransform, 
            deltaTime = deltaTime, 
            Ecb = ecb,
            time = state.WorldUnmanaged.Time.ElapsedTime

        };
        state.Dependency = rangeAttackJob.Schedule(state.Dependency); //cannot be parallel because of structural change
        state.Dependency.Complete();
        
        var throwableParabolaJob = new ThrowableParabolaJob()
        {
            time = state.WorldUnmanaged.Time.ElapsedTime,
            Ecb = ecb.AsParallelWriter(),
            world = collisionWorld,
            Player = SystemAPI.GetComponentLookup<PlayerTagComponent>(true)
        };
        state.Dependency = throwableParabolaJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

    }
    
    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct MoveAndSeparateEnemiesJob : IJobEntity
    {
        public LocalTransform PlayerTransform { get; set; }
        public float deltaTime;
        [field: ReadOnly] public CollisionWorld world { get; set; }
        [field: ReadOnly] public ComponentLookup<ObstacleTag> Obstacles { get; set; }

        [ReadOnly] 
        public ComponentLookup<LocalToWorld> EntityPositions; //Expensive 
        [ReadOnly]
        public ComponentLookup<EnemyTag> Enemies; //Expensive only used to check if hit entity has tag todo --> fix
        
        private const float separationRadius = 1f;
        private const float separationForce = 1f;
        void Execute(in Entity entity, ref LocalTransform localTransform)
        {
            //Normal movement
            Vector3 direction = PlayerTransform.Position - localTransform.Position;
            direction = direction.normalized;

            // Avoid obstacles
             var rayCastInput = new RaycastInput
             {
                 Start = localTransform.Position+(float3)direction,
                 End = localTransform.Position+(((float3)direction*2)),
                 Filter = CollisionFilter.Default
             };
             RaycastHit hit = new RaycastHit();
             bool move = true; 
             if (world.CastRay(rayCastInput, out hit))
             {
                 if (hit.Entity != entity)
                 {
                     if (Obstacles.HasComponent(hit.Entity))
                     {
                         move = false; 
                         Vector3 newDirection = Vector3.zero;
                         Vector3 hitNormal = hit.Position;
                         hitNormal.y = 0.0f;
                         newDirection = Vector3.Reflect(direction, hitNormal);
                         newDirection = newDirection.normalized;

                         // Move in the new direction
                         localTransform.Position += new float3(newDirection * 3 * deltaTime); 
                     }
                 }
             }
             //Normal movement
             if(move)
             {
                 localTransform.Position += new float3(direction * 3 * deltaTime);
                 localTransform.Rotation = Quaternion.LookRotation(direction);
             }

             // Separation
            var result = new NativeList<DistanceHit>(Allocator.TempJob);
            if (world.OverlapSphere(localTransform.Position, separationRadius, ref result, CollisionFilter.Default))
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (Enemies.HasComponent(result[i].Entity) && result[i].Entity != entity)
                    {
                        Vector3 separationDirection = localTransform.Position - EntityPositions.GetRefRO(result[i].Entity).ValueRO.Position;
                        separationDirection = separationDirection.normalized;
                        separationDirection.y = 0f;
                        localTransform.Position += new float3(separationDirection * separationForce * deltaTime);
                    }
                }
            }
            result.Dispose();
        }
    }

    [UpdateAfter(typeof(BulletBehaviourSystem.BulletCollisionJob))]
    [WithAll(typeof(IsDeadComponent))]
    [BurstCompile]
    public partial struct DestroyEnemyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
    
        void Execute([ChunkIndexInQuery] int chunkIndex,in Entity entity)
        {
                ECB.DestroyEntity(chunkIndex,entity);
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(MeleeEnemyTag))]
    public partial struct MeleeAttackJob : IJobEntity
    {
        public float deltaTime;
        public LocalTransform PlayerTransform { get; set; }
        void Execute(in LocalTransform localTransform, ref AttackComponent attack, in MeleeAttackSettingsComponent attackSettings)
        {
            attack.LastAttackTime += deltaTime;
            if (attack.LastAttackTime > attackSettings.MaxTimer)
            {
                float distance = Vector3.Distance(PlayerTransform.Position, localTransform.Position);
                if (distance <= attackSettings.Range)
                {
                    attack.LastAttackTime = 0;
                }
            }
        }
    }
    [BurstCompile]
    [WithAll(typeof(RangeEnemyTag))]
    public partial struct RangeAttackJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;

        public float deltaTime;
        public LocalTransform PlayerTransform { get; set; }
        public double time { get; set; }

        void Execute(in LocalTransform localTransform, ref AttackComponent attack, in RangeAttackSettingsComponent attackSettings, in ThrowableAspect throwableAspect)
        {
            attack.LastAttackTime += deltaTime;
            if (attack.LastAttackTime > attackSettings.MaxTimer)
            {
                float distance = Vector3.Distance(PlayerTransform.Position, localTransform.Position);
                if (distance <= attackSettings.Range)
                {
                    var instance = Ecb.Instantiate(throwableAspect.ThrowablePrefab);
                    Ecb.SetComponent(instance, LocalTransform.FromPosition(localTransform.Position));
                    Ecb.AddComponent<ThrowableTag>(instance);
                    Ecb.AddComponent(instance, new ThrowableSettingsComponent
                    {
                        targetPos = PlayerTransform.Position, 
                        startPos = localTransform.Position,
                        distance = Vector3.Distance(localTransform.Position,PlayerTransform.Position),
                        startTime = time, 
                        terrainHeight = -1.52f,
                        height = 2f
                    });
                    Ecb.AddComponent<IsDeadComponent>(instance);
                    Ecb.SetComponentEnabled(instance,ComponentType.ReadWrite<IsDeadComponent>(), false);
                    attack.LastAttackTime = 0;
                }
            }
        }
    }
    [BurstCompile]
    [WithAll(typeof(ThrowableTag))]
    public partial struct ThrowableParabolaJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [field: ReadOnly] public CollisionWorld world { get; set; }
        [ReadOnly]
        public ComponentLookup<PlayerTagComponent> Player;


        public double time { get; set; }

        void Execute([ChunkIndexInQuery] int chunkIndex, in Entity entity, ref LocalTransform localTransform, in ThrowableSettingsComponent throwableSettingsComponent)
        {
            float distanceSoFar = ((float)time - (float)throwableSettingsComponent.startTime) * 5;
            
            float missingDistance = distanceSoFar / throwableSettingsComponent.distance;
            
            Vector3 currentPos = Vector3.Lerp(throwableSettingsComponent.startPos, throwableSettingsComponent.targetPos, missingDistance);
            float parabolaHeight = throwableSettingsComponent.height * Mathf.Sin(missingDistance * Mathf.PI);
            currentPos.y = Mathf.Max(Mathf.Lerp(throwableSettingsComponent.startPos.y, throwableSettingsComponent.targetPos.y, missingDistance) + parabolaHeight, throwableSettingsComponent.terrainHeight);
            
            localTransform.Position = currentPos;
            
            if (localTransform.Position.y <= throwableSettingsComponent.targetPos.y)
            {
                Ecb.SetComponentEnabled<IsDeadComponent>(chunkIndex, entity,true);
            }
            
            //Collision check
            float radius = .2f;
            var result = new NativeList<DistanceHit>(Allocator.TempJob);
            if (world.OverlapSphere(localTransform.Position, radius, ref result, CollisionFilter.Default))
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (Player.HasComponent(result[i].Entity))
                    {
                        Ecb.SetComponentEnabled<IsDeadComponent>(chunkIndex, entity,true);
                    }
                }
            }
            result.Dispose();
        }
    }
}
