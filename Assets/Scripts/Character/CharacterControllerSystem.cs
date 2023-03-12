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
        var deltaTime = state.WorldUnmanaged.Time.DeltaTime;

        // Debug.Log("sdf");
        var movement = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        var mouseX = Input.GetAxis("Mouse X"); 

        var playerCharacterMoveJob = new PlayerCharacterMoveJob()
        {
            movement = movement,
            mouseX = mouseX,
            deltaTime = deltaTime
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
        public float deltaTime; 
        public void Execute(ref LocalTransform transform)
        {
            float rotationSpeed = 0.2f; 

            quaternion rotation = quaternion.RotateY(mouseX * rotationSpeed);
            transform.Rotation = math.mul(transform.Rotation, rotation);

            float3 rotatedDirection = math.rotate(transform.Rotation, movement);
            transform.Position += rotatedDirection * 5 * deltaTime;
        }
    }
}
