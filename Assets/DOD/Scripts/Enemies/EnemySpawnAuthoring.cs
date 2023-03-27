using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.DOD.Scripts.Enemies
{
    public class EnemySpawnAuthoring : UnityEngine.MonoBehaviour
    {
        public GameObject meleePrefab;
        public int amount;
        class EnemyBaker : Baker<EnemySpawnAuthoring>
        {
            public override void Bake(EnemySpawnAuthoring authoring)
            {
                AddComponent(new EnemyPrefabs
                {
                    MeleePrefab = GetEntity(authoring.meleePrefab),
                } );
                AddComponent(new EnemySpawnSettings
                {
                    Amount = authoring.amount, 
                    SpawnPosition = authoring.gameObject.transform.position
                } );
            }
        }
    }
    
    public struct EnemyPrefabs : IComponentData
    {
        public Entity MeleePrefab;
        //Todo --> add new enemy types here
    }
    public struct EnemySpawnSettings : IComponentData
    {
        public int Amount;
        public float3 SpawnPosition;
        //todo --> add type parameter to define the type of enemy to be spawned 
    }
}
