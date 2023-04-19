using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
    private Random generator;
    EntityQuery dayNightQuery;


    public void OnCreate(ref SystemState state)
    {
        dayNightQuery = new EntityQueryBuilder(state.WorldUpdateAllocator)
            .WithAll<DayNightComponent>().Build(ref state);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var dayNight = dayNightQuery.ToComponentDataArray<DayNightComponent>(Allocator.Temp);
        if (dayNight[0].isNight)
        {
            if (!dayNight[0].enemiesHasSpawned)
            {
                generator = new Random((uint) UnityEngine.Random.Range(-10000, 10000));

                var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
                var spawnEnemyJob = new SpawnEnemyJob
                {
                    Ecb = ecb,
                    generator = generator,
                    cycleNumber = dayNight[0].dayNightCycleNumber
                };
                spawnEnemyJob.Schedule();

                //Destroy the loot
                var dayNightSpawnJob = new DayNightSystem.DayNightSpawnParameterJob().ScheduleParallel(state.Dependency);
                dayNightSpawnJob.Complete();
                
                var destroyLootJob = new LootBehaviorSystem.DestroyLootJob
                {
                    ecb = ecb.AsParallelWriter()
                };
                state.Dependency = destroyLootJob.ScheduleParallel(state.Dependency);
                state.Dependency.Complete();
            }
        }
    }
    
    [BurstCompile]
    public partial struct SpawnEnemyJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public Random generator;
        public int cycleNumber { get; set; }

        void Execute(in EnemySpawnAspect enemySpawnAspect)
        {
         Vector3 spawnAreaSize = new Vector3(10, 0, 10);
         
            for (int i = 0; i < enemySpawnAspect.MeleeAmount*cycleNumber; i++)
            {
                float3 spawnPosition = enemySpawnAspect.SpawnPosition + new float3(generator.NextFloat(-spawnAreaSize.x, spawnAreaSize.x), 0, generator.NextFloat(-spawnAreaSize.z, spawnAreaSize.z));

                var instance = Ecb.Instantiate(enemySpawnAspect.MeleePrefab);
                var enemyTransForm = LocalTransform.FromPosition(spawnPosition);
                Ecb.SetComponent(instance,enemyTransForm);
                Ecb.AddComponent(instance, new EnemyTag());
                Ecb.AddComponent(instance, new HealthComponent
                {
                    value = 10
                });
                Ecb.AddComponent<IsDeadComponent>(instance);
                Ecb.SetComponentEnabled(instance,ComponentType.ReadWrite<IsDeadComponent>(), false);
                Ecb.AddComponent(instance, new AttackComponent());
                Ecb.AddSharedComponent(instance, new MeleeAttackSettingsComponent
                {
                    Range = 1.5f, 
                    MaxTimer = 1f
                });
            }
            
            for (int i = 0; i < enemySpawnAspect.RangeAmount*cycleNumber; i++)
            {
                float3 spawnPosition = enemySpawnAspect.SpawnPosition + new float3(generator.NextFloat(-spawnAreaSize.x, spawnAreaSize.x), 0, generator.NextFloat(-spawnAreaSize.z, spawnAreaSize.z));
            
                var instance = Ecb.Instantiate(enemySpawnAspect.RangePrefab);
                var enemyTransForm = LocalTransform.FromPosition(spawnPosition);
                Ecb.SetComponent(instance,enemyTransForm);
                Ecb.AddComponent(instance, new EnemyTag());
                Ecb.AddComponent(instance, new HealthComponent
                {
                    value = 20
                });
                Ecb.AddComponent<IsDeadComponent>(instance);
                Ecb.SetComponentEnabled(instance,ComponentType.ReadWrite<IsDeadComponent>(), false);
                Ecb.AddComponent(instance, new AttackComponent());
                Ecb.AddSharedComponent(instance, new RangeAttackSettingsComponent
                {
                    Range = 30f, 
                    MaxTimer = 3f
                });
            }
        }
    }
}
