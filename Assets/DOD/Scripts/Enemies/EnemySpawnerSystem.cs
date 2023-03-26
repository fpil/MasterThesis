using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;


public partial struct EnemySpawnerSystem : ISystem
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
        generator = new Random((uint) UnityEngine.Random.Range(-10000, 10000));

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        var spawnEnemyJob = new SpawnEnemyJob
        {
            Ecb = ecb,
            generator = generator,
        };
        spawnEnemyJob.Run();
        //Disables the system update
        state.Enabled = false;
    }
    
    public partial struct SpawnEnemyJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public Random generator;
        
        void Execute(in EnemySpawnAspect enemySpawnAspect)
        {
         Vector3 spawnAreaSize = new Vector3(10, 0, 10);

            for (int i = 0; i < enemySpawnAspect.Amount; i++)
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
                // Ecb.AddComponent(instance,  typeof(IsDeadComponent));
                Ecb.AddComponent<IsDeadComponent>(instance);
                Ecb.SetComponentEnabled(instance,typeof(IsDeadComponent), false);
                //Add more components 
            }
        }
    }
}
