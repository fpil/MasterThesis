using System.Collections;
using System.Collections.Generic;
using Assets.DOD.Scripts.Enemies;
using Unity.Entities;
using UnityEngine;

public class LootSpawnerAuthoring : MonoBehaviour
{
    public GameObject shotgunLoot;
    public GameObject machinegunLoot;
    public int spawnRate;
    public int spawnRateItem;
    
    class LootSpawnerBaker : Baker<LootSpawnerAuthoring>
    {
        public override void Bake(LootSpawnerAuthoring authoring)
        {
            AddComponent(new LootPrefabs
            {
                shotgunLootPrefab = GetEntity(authoring.shotgunLoot),
                machinegunLootPrefab = GetEntity(authoring.machinegunLoot),
                spawnRate = authoring.spawnRate, 
                spawnRateItem = authoring.spawnRateItem
            });
        }
    }
    
    public struct LootPrefabs : IComponentData
    {
        public Entity shotgunLootPrefab;
        public Entity machinegunLootPrefab;
        public int spawnRate;
        public int spawnRateItem;
    }
}
