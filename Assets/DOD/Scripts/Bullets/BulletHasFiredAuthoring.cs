using Unity.Entities;
using Unity.Mathematics;

namespace DOD.Scripts.Bullets
{
    public class BulletHasFiredAuthoring : UnityEngine.MonoBehaviour
    {
        class CannonBallBaker : Baker<BulletHasFiredAuthoring>
        {
            public override void Bake(BulletHasFiredAuthoring authoring)
            {
                // By default, components are zero-initialized.
                // So in this case, the Speed field in CannonBall will be float3.zero.
                AddComponent<BulletFired>();
            }
        }
    }

    public struct BulletFired : IComponentData
    {
        public int _hasFired;
        public float3 fireDirection;
    }
}