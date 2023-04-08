using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public partial struct DayNightSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
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
    }
    
    [BurstCompile]
    public partial struct DayNightCycleJob : IJobEntity
    {
        public float deltaTime;
        public void Execute(ref DayNightComponent dayNightComponent)
        {
            dayNightComponent.dayTime += deltaTime;
            Debug.Log(dayNightComponent.dayTime);
        }
    }
}
