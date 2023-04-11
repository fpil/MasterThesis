using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct LootBehaviorSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = state.WorldUnmanaged.Time.DeltaTime;
        var elaspedTime = state.WorldUnmanaged.Time.ElapsedTime;
        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;


        var lootMovementJob = new LootMovementJob
        {
            deltaTime = deltaTime,
            elapsedTime = elaspedTime,
            world = collisionWorld, 
            player = SystemAPI.GetComponentLookup<PlayerTagComponent>(true),
            // machinegunLoot = SystemAPI.GetComponentLookup<MachinegunLootTag>(true),
            shotgunLoot = SystemAPI.GetComponentLookup<ShotgunLootTag>(true),
        };
        state.Dependency = lootMovementJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
    }
    
    [BurstCompile]
    public partial struct LootMovementJob : IJobEntity
    {
        public float deltaTime { get; set; }
        public double elapsedTime { get; set; }

        private const float separationRadius = 0.50f;
        [field: ReadOnly] public CollisionWorld world { get; set; }
        [field: ReadOnly] public ComponentLookup<PlayerTagComponent> player { get; set; }
        // [field: ReadOnly] public ComponentLookup<MachinegunLootTag> machinegunLoot { get; set; }
        [field: ReadOnly] public ComponentLookup<ShotgunLootTag> shotgunLoot { get; set; }


        void Execute(in Entity entity, in LootTag lootTag, ref LocalTransform localTransform)
        {
            localTransform.Rotation = math.mul(localTransform.Rotation, quaternion.RotateY(1.0f*deltaTime));
            
            var result = new NativeList<DistanceHit>(Allocator.TempJob);
            if (world.OverlapSphere(localTransform.Position, separationRadius, ref result, CollisionFilter.Default))
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (player.HasComponent(result[i].Entity))
                    {
                        if (shotgunLoot.HasComponent(entity))
                        {
                        }
                        else
                        {
                        }
                    }
                }
            }
            result.Dispose();
        }
    }
}
