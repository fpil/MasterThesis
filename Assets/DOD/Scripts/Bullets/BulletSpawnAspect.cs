using Unity.Entities;

// Instead of directly accessing the Turret component, we are creating an aspect.
// Aspects allows you to provide a customized API for accessing your components.
public readonly partial struct BulletSpawnAspect : IAspect
{
    readonly RefRO<BulletSpawnPositionComponent> m_Bullet;
    readonly RefRW<Ammo> m_Ammo;

    // Note the use of ValueRO in the following properties.
    public Entity BulletPrefab => m_Bullet.ValueRO.BulletPrefab;
    // public int ShotgunAmmo => m_Ammo.ValueRW.ShotgunAmmo;
    // public int MachinegunAmmo => m_Ammo.ValueRW.MachineGunAmmo;

    public int ShotgunAmmo
    {
        get => m_Ammo.ValueRO.ShotgunAmmo;
        set => m_Ammo.ValueRW.ShotgunAmmo = value;

    }
    public int MachineGunAmmo
    {
        get => m_Ammo.ValueRO.MachineGunAmmo;
        set => m_Ammo.ValueRW.MachineGunAmmo = value;

    }
}