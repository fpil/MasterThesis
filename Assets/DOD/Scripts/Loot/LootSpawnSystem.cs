using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;


public partial struct LootSpawnSystem : ISystem
{
    EntityQuery killedEnemyQuery;
    private Random generator;

    public void OnCreate(ref SystemState state)
    {
        killedEnemyQuery = new EntityQueryBuilder(state.WorldUpdateAllocator)
            .WithAll<EnemyTag>()
            .WithAll<IsDeadComponent>()
            .WithAll<LocalTransform>()
            .Build(ref state);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var killedEnemies = killedEnemyQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        if (killedEnemies.Length > 0)
        {
            generator = new Random((uint) UnityEngine.Random.Range(-10000, 10000));
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var killedEnemy in killedEnemies)
            {
                var spawnEnemyJob = new SpawnLootJob
                {
                    Ecb = ecb,
                    generator = generator,
                    spawnPosition = killedEnemy.Position
                };
                spawnEnemyJob.Schedule();
            }
        }
    }
    
     public partial struct SpawnLootJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public Random generator;
        public float3 spawnPosition;

        void Execute(in LootSpawnerAuthoring.LootPrefabs lootPrefabs)
        {
            int maxSpawnRate = 101;
            int spawnChance = generator.NextInt(0, maxSpawnRate);
            if (spawnChance <= lootPrefabs.spawnRate)
            {
                Entity instance;
                int spawnChanceItem = generator.NextInt(0, maxSpawnRate);
                if (lootPrefabs.spawnRateItem <= spawnChanceItem)
                {
                    instance = Ecb.Instantiate(lootPrefabs.shotgunLootPrefab);
                }
                else
                {
                    instance = Ecb.Instantiate(lootPrefabs.machinegunLootPrefab);
                }
                var transform = LocalTransform.FromPosition(spawnPosition);
                Ecb.SetComponent(instance,transform);
                // Ecb.AddComponent<IsDeadComponent>(instance);
            }
        }
    }
}
