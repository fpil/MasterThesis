using Unity.Entities;
using UnityEngine;

namespace DOD.Scripts.Enemies
{
    public class RangeEnemyAuthoring : UnityEngine.MonoBehaviour
    {
        public GameObject throwablePrefab;
        class EnemyTagBaker : Baker<RangeEnemyAuthoring>
        {
            public override void Bake(RangeEnemyAuthoring authoring)
            {
                AddComponent<RangeEnemyTag>();
                AddComponent(new ThrowableComponent
                {
                    ThrowablePrefab = GetEntity(authoring.throwablePrefab)
                });
            }
        }
    }
    public struct RangeEnemyTag : IComponentData
    {
    }

    public struct ThrowableComponent : IComponentData
    {
        public Entity ThrowablePrefab;
    }
}