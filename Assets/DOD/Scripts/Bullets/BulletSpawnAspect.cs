using Unity.Entities;

// Instead of directly accessing the Turret component, we are creating an aspect.
// Aspects allows you to provide a customized API for accessing your components.
public readonly partial struct BulletSpawnAspect : IAspect
{
    readonly RefRO<BulletSpawnPositionComponent> m_Bullet;
    // RefRW<BulletFired> m_hasFired; 

    // Note the use of ValueRO in the following properties.
    public Entity BulletPrefab => m_Bullet.ValueRO.BulletPrefab;
}