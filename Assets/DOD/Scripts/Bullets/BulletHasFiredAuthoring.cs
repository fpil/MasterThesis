using Unity.Entities;

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

    struct BulletFired : IComponentData
    {
        public int _hasFired;
    }
}