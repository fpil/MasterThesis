using Unity.Entities;
using Unity.Mathematics;
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
        
        var moveTowardPlayerJob = new MoveTowardPlayerJob()
        {
            PlayerTransform = playerTransform,
            deltaTime = deltaTime
        };
        state.Dependency = moveTowardPlayerJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

    }
    
    [WithAll(typeof(EnemyTag))]
    public partial struct MoveTowardPlayerJob : IJobEntity
    {
        public LocalTransform PlayerTransform { get; set; }
        public float deltaTime;
        void Execute(ref LocalTransform localTransform)
        {
            Vector3 direction = (PlayerTransform.Position - localTransform.Position);
            direction = direction.normalized;
            localTransform.Position += new float3(direction * 5 * deltaTime);
            localTransform.Rotation = Quaternion.LookRotation(direction);
        }
    }
}
