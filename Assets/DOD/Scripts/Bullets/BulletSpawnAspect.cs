using DOD.Scripts.Bullets;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

// Instead of directly accessing the Turret component, we are creating an aspect.
// Aspects allows you to provide a customized API for accessing your components.
public readonly partial struct BulletSpawnAspect : IAspect
{
    // This reference provides read only access to the Turret component.
    // Trying to use ValueRW (instead of ValueRO) on a read-only reference is an error.
    readonly RefRO<BulletSpawnPositionComponent> m_Bullet;
    // RefRW<BulletFired> m_hasFired; 

    // Note the use of ValueRO in the following properties.
    public Entity BulletPrefab => m_Bullet.ValueRO.BulletPrefab;
}