using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;


public partial struct LootBehaviorSystem : ISystem
{
    private Random generator;

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
        generator = new Random((uint) UnityEngine.Random.Range(-10000, 10000));
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var lootMovementJob = new LootBehaviorJob
        {
            deltaTime = deltaTime,
            elapsedTime = elaspedTime,
            world = collisionWorld, 
            player = SystemAPI.GetComponentLookup<PlayerTagComponent>(true),
            shotgunLoot = SystemAPI.GetComponentLookup<ShotgunLootTag>(true),
            bulletController = SystemAPI.GetSingletonRW<Ammo>(), 
            generator = generator, 
            ecb = ecb.AsParallelWriter()
        };
        state.Dependency = lootMovementJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
    }
    
    [BurstCompile]
    public partial struct LootBehaviorJob : IJobEntity
    {
        public float deltaTime { get; set; }
        public double elapsedTime { get; set; }

        private const float separationRadius = 0.50f;
        [field: ReadOnly] public CollisionWorld world { get; set; }
        [field: ReadOnly] public ComponentLookup<PlayerTagComponent> player { get; set; }
        [field: ReadOnly] public ComponentLookup<ShotgunLootTag> shotgunLoot { get; set; }
        [field: NativeDisableUnsafePtrRestriction] public RefRW<Ammo> bulletController { get; set; }
        public Random generator { get; set; }
        public EntityCommandBuffer.ParallelWriter ecb { get; set; }


        void Execute([ChunkIndexInQuery] int chunkIndex,in Entity entity, in LootTag lootTag, ref LocalTransform localTransform)
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
                            bulletController.ValueRW.ShotgunAmmo += generator.NextInt(10, 30);
                        }
                        else
                        {
                            bulletController.ValueRW.MachineGunAmmo += generator.NextInt(30, 100);
                        }
                        ecb.DestroyEntity(chunkIndex,entity);
                    }
                }
            }
            result.Dispose();
        }
    }
    
    [BurstCompile]
    public partial struct DestroyLootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb { get; set; }
        void Execute([ChunkIndexInQuery] int chunkIndex,in Entity entity, in LootTag lootTag)
        {
            ecb.DestroyEntity(chunkIndex,entity);
           
        }
    }
}
