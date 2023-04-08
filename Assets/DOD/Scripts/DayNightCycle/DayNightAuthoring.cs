using Unity.Entities;
using UnityEngine;

public class DayNightAuthoring : MonoBehaviour
{
    public float dayTime;
    public float maxDayTime;
    public bool isNight;
    public bool enemiesHasSpawned;
    public int enemiesLeft;
    public int dayNightCycleNumber;
    class DayNightBaker : Baker<DayNightAuthoring>
    {
        public override void Bake(DayNightAuthoring authoring)
        {
            AddComponent(new DayNightComponent
            {
                dayTime = authoring.dayTime,
                maxDayTime = authoring.maxDayTime,
                isNight = authoring.isNight,
                enemiesHasSpawned = authoring.enemiesHasSpawned,
                enemiesLeft = authoring.enemiesLeft,
                dayNightCycleNumber = authoring.dayNightCycleNumber
            }
            );
        }
    }
}

public struct DayNightComponent : IComponentData
{
    public float dayTime;
    public float maxDayTime;
    public bool isNight;
    public bool enemiesHasSpawned;
    public int enemiesLeft;
    public int dayNightCycleNumber;
}
