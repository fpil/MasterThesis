using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

// [BurstCompile]
public partial struct CharacterControllerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }
    
    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = state.WorldUnmanaged.Time.DeltaTime;

        var movement = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        var cameraTransform = Camera.main.transform.rotation; 
        
        var playerCharacterMoveJob = new PlayerCharacterMoveJob
        {
            cameraTransform = cameraTransform,
            movement = movement,
            deltaTime = deltaTime 
        };
        // playerCharacterMoveJob.Run(); //main thread
        state.Dependency = playerCharacterMoveJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

    }
    [WithAll(typeof(PlayerTagComponent))]
    // [BurstCompile]
    public partial struct PlayerCharacterMoveJob : IJobEntity
    {
        public Quaternion cameraTransform; 
        public float3 movement;
        public float deltaTime;
        public void Execute(ref LocalTransform transform)
        {
            //Set the rotation of the character to the camera rotation
            Quaternion playerTransformRotation = cameraTransform;
            playerTransformRotation.x = 0;
            playerTransformRotation.z = 0;
            
            //Smoothness
            transform.Rotation = Quaternion.Lerp(transform.Rotation, playerTransformRotation, 0.1f);
            //Move the character towards its orientation
            float3 rotatedDirection = math.rotate(transform.Rotation, movement);
            rotatedDirection.y = 0;
            transform.Position += rotatedDirection * 5 * deltaTime;
        }
    }
}
