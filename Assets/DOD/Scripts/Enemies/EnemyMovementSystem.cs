using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct EnemyMovementSystem : ISystem
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

        
        var moveTowardPlayerJob = new MoveTowardPlayerJob()
        {
            PlayerTransform = playerTransform,
            deltaTime = deltaTime,
        };
        // moveTowardPlayerJob.Run();
        state.Dependency = moveTowardPlayerJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();


        // var allLocalTransform = state.World.EntityManager.GetComponentData<LocalTransform>(true); 
        
        var enemySeparationJob = new EnemySeparationJob()
        {
            deltaTime = deltaTime,
            world = collisionWorld,
            entityManager = state.World.EntityManager
        };
        enemySeparationJob.Run();
        // state.Dependency = enemySeparationJob.ScheduleParallel(state.Dependency);
        // state.Dependency.Complete();

    }
    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct MoveTowardPlayerJob : IJobEntity
    {
        public LocalTransform PlayerTransform { get; set; }
        public float deltaTime;
        void Execute(ref LocalTransform localTransform)
        {
            Vector3 direction = (PlayerTransform.Position - localTransform.Position);
            direction = direction.normalized;
            localTransform.Position += new float3(direction * 5 * deltaTime); //Todo --> adjust the speed to be specific to the enemy
            localTransform.Rotation = Quaternion.LookRotation(direction);
        }
    }

    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct EnemySeparationJob : IJobEntity
    {
        public float deltaTime;
        [field: ReadOnly] public CollisionWorld world { get; set; }
        public EntityManager entityManager;
        // [ReadOnly] public ComponentTypeHandle<LocalTransform> LocalTransformType;

        void Execute(in Entity entity, ref LocalTransform localTransform)
        {
            
            float separationRadius = 2f;
            float separationForce = 1f;
            var result = new NativeList<DistanceHit>(Allocator.TempJob);
            if (world.OverlapSphere(localTransform.Position, separationRadius, ref result, CollisionFilter.Default))
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (entityManager.HasComponent<EnemyTag>(result[i].Entity) && result[i].Entity != entity)
                    {
                       
                            // Get the LocalTransform component from the target entity
                        
                            // var targetLookup = SystemAPI.GetComponentLookup<LocalTransform>();
                            // var targetLookup = SystemAPI.GetComponent<LocalTransform>(result[i].Entity); 
                            // var entityTransform = targetLookup.GetRefRO(result[i].Entity);
                            // Debug.Log(targetLookup.Position);
                            // var targetTransform = entityManager.GetComponentData<LocalTransform>(result[i].Entity);
                            // Debug.Log(targetTransform.Position);

                            // Use the LocalTransform data from the target entity in your job
                            // ...
                        
                        // var collider = entityManager.GetComponentData<LocalTransform>(result[i].Entity);
                                
                        // Debug.Log(collider.Position);
                        Vector3 separationDirection = (localTransform.Position - result[i].Position);
                        separationDirection = separationDirection.normalized;
                        separationDirection.y = 0f;
                        localTransform.Position += new float3(separationDirection * separationForce * deltaTime);
                    }
                }
            }

            result.Dispose();



            // foreach (Collider collider in colliders) {
            //     if (collider.gameObject.CompareTag("MeeleEnemy") && collider.gameObject != gameObject) { //Todo --> change the default tag when more enemies are added
            //         Vector3 separationDirection = (transform.position - collider.transform.position).normalized;
            //         separationDirection.y = 0f;
            //         transform.position += separationDirection * separationForce * Time.deltaTime;
            //     }
            // }
        }
    }
}
