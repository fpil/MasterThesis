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

        var moveTowardPlayerJob = new MoveTowardPlayerJob
        {
            PlayerTransform = playerTransform,
            deltaTime = deltaTime
        };
        state.Dependency = moveTowardPlayerJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        var enemySeparationJob = new EnemySeparationJob
        {
            deltaTime = deltaTime,
            world = collisionWorld,
            EntityPositions = SystemAPI.GetComponentLookup<LocalToWorld>(true), //Possibly Expensive
            Enemies = SystemAPI.GetComponentLookup<EnemyTag>(true) //Possibly Expensive
        };
        state.Dependency = enemySeparationJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
        
        var destroyEnemyJob = new DestroyEnemyJob
        {
            ECB = ecb.AsParallelWriter()
        };
        state.Dependency = destroyEnemyJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
        
        var meleeAttackJob = new MeleeAttackJob
        {
            world = collisionWorld, 
            Player = SystemAPI.GetComponentLookup<PlayerTagComponent>(true)
        };
        state.Dependency = meleeAttackJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
    }
    
    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct MoveTowardPlayerJob : IJobEntity
    {
        public LocalTransform PlayerTransform { get; set; }
        public float deltaTime;
        void Execute(ref LocalTransform localTransform)
        {
            Vector3 direction = PlayerTransform.Position - localTransform.Position;
            direction = direction.normalized;
            localTransform.Position += new float3(direction * 3 * deltaTime); //Todo --> adjust the speed to be specific to the enemy
            localTransform.Rotation = Quaternion.LookRotation(direction);
        }
    }

    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct EnemySeparationJob : IJobEntity
    {
        public float deltaTime;
        [field: ReadOnly] public CollisionWorld world { get; set; }
        [ReadOnly] 
        public ComponentLookup<LocalToWorld> EntityPositions;
        [ReadOnly]
        public ComponentLookup<EnemyTag> Enemies;
        
        private const float separationRadius = 1f;
        private const float separationForce = 1f;

        //Todo --> this is slowing down the application = Expensive script
        void Execute(in Entity entity, ref LocalTransform localTransform)
        {
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
    
    
    //This is slow //todo --> fix 
    [BurstCompile]
    [WithAll(typeof(MeleeEnemyTag))]
    public partial struct MeleeAttackJob : IJobEntity
    {
        [field: ReadOnly] public CollisionWorld world { get; set; }
        [ReadOnly]
        public ComponentLookup<PlayerTagComponent> Player;

        void Execute(in Entity entity, in LocalTransform localTransform)
        {
            var rayCastInput = new RaycastInput
            {
                Start = localTransform.Position + localTransform.Forward(),
                End = localTransform.Position + (localTransform.Forward()*1.5f),
                Filter = CollisionFilter.Default
            };
            
            RaycastHit hit = new RaycastHit();
            if (world.CastRay(rayCastInput, out hit))
            {
                if (hit.Entity != entity)
                {
                    if (Player.HasComponent(hit.Entity))
                    {
                        Debug.Log("Attack player");
                    }
                }
            }
        }
    }
}
