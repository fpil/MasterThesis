using Assets.DOD.Scripts.Enemies;
using Unity.Entities;
using Unity.Mathematics;

public readonly partial struct EnemySpawnAspect : IAspect
{
    readonly RefRO<EnemyPrefabs> m_MeleeEnemy;
    readonly RefRO<EnemySpawnSettings> m_SpawnSettings;

    // Note the use of ValueRO in the following properties.
    public Entity MeleePrefab => m_MeleeEnemy.ValueRO.MeleePrefab;
    public Entity RangePrefab => m_MeleeEnemy.ValueRO.RangePrefab;
    public float3 SpawnPosition => m_SpawnSettings.ValueRO.SpawnPosition;
    public int MeleeAmount => m_SpawnSettings.ValueRO.MeleeAmount;
    public int RangeAmount => m_SpawnSettings.ValueRO.RangeAmount;
}
