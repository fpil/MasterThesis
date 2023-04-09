using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial struct DayNightSystem : ISystem
{
    EntityQuery enemiesQuery;
    EntityQuery dayNightQuery;
    public void OnCreate(ref SystemState state)
    {
        enemiesQuery = new EntityQueryBuilder(state.WorldUpdateAllocator)
            .WithAll<EnemyTag>().Build(ref state);
        dayNightQuery = new EntityQueryBuilder(state.WorldUpdateAllocator)
            .WithAll<DayNightComponent>().Build(ref state);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = state.WorldUnmanaged.Time.DeltaTime;
        var dayNightCycleJob = new DayNightCycleJob
        {
            deltaTime = deltaTime 
        };
        state.Dependency = dayNightCycleJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
        
        
        var enemies = enemiesQuery.ToComponentDataArray<EnemyTag>(Allocator.Temp);
        var dayNight = dayNightQuery.ToComponentDataArray<DayNightComponent>(Allocator.Temp); //Only one should exist
        if (enemies.Length == 0 && dayNight[0].enemiesHasSpawned)
        {
            var updateDayNightParametersJob = new UpdateDayNightParametersJob();
            state.Dependency = updateDayNightParametersJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
        }

        enemies.Dispose();
        dayNight.Dispose();
    }
    
    [BurstCompile]
    public partial struct DayNightCycleJob : IJobEntity
    {
        public float deltaTime;
        public void Execute(ref DayNightComponent dayNightComponent)
        {
            dayNightComponent.dayTime += deltaTime;
            if (dayNightComponent.dayTime > dayNightComponent.maxDayTime)
            {
                if (!dayNightComponent.isNight)
                {
                    dayNightComponent.isNight = true;
                }
            }
        }
    }
    [BurstCompile]
    public partial struct UpdateDayNightParametersJob : IJobEntity
    {
        public void Execute(ref DayNightComponent dayNightComponent)
        {
            dayNightComponent.dayTime = 0;
            dayNightComponent.isNight = false;
            dayNightComponent.enemiesHasSpawned = false;
            dayNightComponent.dayNightCycleNumber++;
        }
    }
    
    [BurstCompile]
    public partial struct DayNightSpawnParameterJob : IJobEntity
    {
        public void Execute(ref DayNightComponent dayNightComponent)
        {
            dayNightComponent.enemiesHasSpawned = true;
        }
    }
}
