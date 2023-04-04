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
            }
        }
    }
    
    public struct EnemyPrefabs : IComponentData
    {
        public Entity MeleePrefab;
        public Entity RangePrefab;
        //Todo --> add new enemy types here
    }
    public struct EnemySpawnSettings : IComponentData
    {
        public int MeleeAmount;
        public int RangeAmount;
        public float3 SpawnPosition;
        //todo --> add type parameter to define the type of enemy to be spawned 
    }
}
