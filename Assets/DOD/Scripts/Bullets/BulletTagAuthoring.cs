using Unity.Entities;
using UnityEngine;

namespace Assets.DOD.Scripts.Bullets
{
    public class BulletTagAuthoring : MonoBehaviour
    {
    
    }
    class BulletBaker : Baker<BulletTagAuthoring>
    {
        public override void Bake(BulletTagAuthoring authoring)
        {
            AddComponent(new BulletTag());
        }
    }
}