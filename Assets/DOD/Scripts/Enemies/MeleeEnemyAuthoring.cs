using Unity.Entities;

namespace DOD.Scripts.Enemies
{
    public class MeleeEnemyAuthoring : UnityEngine.MonoBehaviour
    {
        class EnemyTagBaker : Baker<MeleeEnemyAuthoring>
        {
            public override void Bake(MeleeEnemyAuthoring authoring)
            {
                AddComponent<MeleeEnemyTag>();
            }
        }
    }
    public struct MeleeEnemyTag : IComponentData
    {
    }
}