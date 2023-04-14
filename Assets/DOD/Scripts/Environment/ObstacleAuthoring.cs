using Assets.DOD.Scripts.Enemies;
using Unity.Entities;
using UnityEngine;

namespace DOD.Scripts.Environment
{
    public class ObstacleAuthoring : MonoBehaviour
    {
        class EnemyBaker : Baker<ObstacleAuthoring>
        {
            public override void Bake(ObstacleAuthoring authoring)
            {
                AddComponent(new ObstacleTag());
            }
        }
    }
    public struct ObstacleTag : IComponentData
    {
    }
}
