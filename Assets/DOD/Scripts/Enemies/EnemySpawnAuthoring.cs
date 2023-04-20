using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.DOD.Scripts.Enemies
{
    public class EnemySpawnAuthoring : UnityEngine.MonoBehaviour
    {
        public GameObject meleePrefab;
        public GameObject rangePrefab;
        public int meleeAmount;
        public int rangeAmount;
        public GameObject throwablePrefab;
        class EnemyBaker : Baker<EnemySpawnAuthoring>
        {
            public override void Bake(EnemySpawnAuthoring authoring)
            {
                AddComponent(new EnemyPrefabs
                {
                    MeleePrefab = GetEntity(authoring.meleePrefab),
                    RangePrefab = GetEntity(authoring.rangePrefab)
                } );
                AddComponent(new EnemySpawnSettings
                {
                    MeleeAmount = authoring.meleeAmount, 
                    RangeAmount = authoring.rangeAmount, 
                    SpawnPosition = authoring.gameObject.transform.position
                } );
                AddComponent(new ThrowablePrefabs
                {
                    ThrowablePrefab = GetEntity(authoring.throwablePrefab),
                } );
            }
        }
    }
    
    public struct EnemyPrefabs : IComponentData
    {
        public Entity MeleePrefab;
        public Entity RangePrefab;
    }
    public struct ThrowablePrefabs : IComponentData
    {
        public Entity ThrowablePrefab;
    }
    public struct EnemySpawnSettings : IComponentData
    {
        public int MeleeAmount;
        public int RangeAmount;
        public float3 SpawnPosition;
    }
}
