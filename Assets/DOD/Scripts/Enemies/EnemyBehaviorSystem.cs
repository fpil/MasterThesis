using Assets.DOD.Scripts.Enemies;
using DOD.Scripts.Bullets;
using DOD.Scripts.Enemies;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;


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

    public void OnUpdate(ref SystemState state)
    {
        Entity playerEntity = playerQuery.GetSingletonEntity(); //Maybe not the best approach to use a singleton
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
            Enemies = SystemAPI.GetComponentLookup<EnemyTag>(true) //Possibly Expensive
        };
        state.Dependency = moveTowardPlayerJob.ScheduleParallel(state.Dependency);
        // state.Dependency.Complete();
        
        
        var destroyEnemyJob = new DestroyEnemyJob
        {
            ECB = ecb.AsParallelWriter()
        };
        state.Dependency = destroyEnemyJob.ScheduleParallel(state.Dependency);
        // state.Dependency.Complete();
        
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
        // rangeAttackJob.Run();
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
        [ReadOnly] 
        public ComponentLookup<LocalToWorld> EntityPositions; //Expensive 
        [ReadOnly]
        public ComponentLookup<EnemyTag> Enemies; //Expensive only used to check if hit entity has tag todo --> fix
        
        private const float separationRadius = 1f;
        private const float separationForce = 1f;
        void Execute(in Entity entity, ref LocalTransform localTransform)
        {
            Vector3 direction = PlayerTransform.Position - localTransform.Position;
            direction = direction.normalized;
            localTransform.Position += new float3(direction * 3 * deltaTime); //Todo --> adjust the speed to be specific to the enemy
            localTransform.Rotation = Quaternion.LookRotation(direction);
            
            
            // Separation
            var result = new NativeList<DistanceHit>(Allocator.TempJob);
            if (world.OverlapSphere(localTransform.Position, separationRadius, ref result, CollisionFilter.Default))
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (Enemies.HasComponent(result[i].Entity) && result[i].Entity != entity)
                    {
                        Vector3 separationDirection = localTransform.Position - EntityPositions.GetRefRO(result[i].Entity).ValueRO.Position;
                        // Vector3 separationDirection = localTransform.Position - result[i].Position;
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
    [BurstCompile]
    [WithAll(typeof(IsDeadComponent))]
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
                    Debug.Log("Attack");
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
                    Ecb.SetComponentEnabled(instance,typeof(IsDeadComponent), false);
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
                        Debug.Log("Hit");
                        Ecb.SetComponentEnabled<IsDeadComponent>(chunkIndex, entity,true);
                    }
                }
            }
            result.Dispose();
        }
    }
}
