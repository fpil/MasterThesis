using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

[BurstCompile]
public partial struct CharacterControllerSystem : ISystem
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
        // var deltaTime = state.WorldUnmanaged.Time.DeltaTime;

        // Debug.Log("sdf");
        var movement = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        var mouseX = Input.GetAxis("Mouse X"); 

        var playerCharacterMoveJob = new PlayerCharacterMoveJob()
        {
            movement = movement,
            mouseX = mouseX
        };
        // playerCharacterMoveJob.Run();
        // playerCharacterMoveJob.Run();

        state.Dependency = playerCharacterMoveJob.ScheduleParallel(state.Dependency);

    }
    
    [WithAll(typeof(PlayerTagComponent))]
    [BurstCompile]
    public partial struct PlayerCharacterMoveJob : IJobEntity
    {
        public float3 movement;
        public float mouseX;
        public void Execute(ref LocalTransform transform)
        {
            transform.Position += movement;
            transform.Rotate(); //todo --> fix this
            // var movement = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            //
            // Debug.Log(transform);
        }
    }
}
