using DOD.Scripts.Enemies;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(EnemySpawnerSystem))]
[BurstCompile]
public partial struct ThrowablePoolSystem : ISystem
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
        

    }
}
