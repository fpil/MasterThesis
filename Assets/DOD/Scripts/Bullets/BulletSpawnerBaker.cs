using DOD.Scripts.Bullets;
using Unity.Entities;
using UnityEngine;

namespace DOD.Scripts.Bullets
{
    public class BulletSpawnerBaker : MonoBehaviour
    {
        public Transform bulletPrefab; 
        public int shotgunAmmo; 
        public int machineGunAmmo; 
    }
}

public struct BulletSpawnPositionComponent : IComponentData
{
    public Entity BulletPrefab;
}
public struct Ammo : IComponentData
{
    public int ShotgunAmmo;
    public int MachineGunAmmo;
}


public class BulletBaker : Baker<BulletSpawnerBaker>
{
    public override void Bake(BulletSpawnerBaker authoring)
    {
        AddComponent(new BulletSpawnPositionComponent
        {
            BulletPrefab = GetEntity(authoring.bulletPrefab),
        } );
        AddComponent(new Ammo
        {
            ShotgunAmmo = authoring.shotgunAmmo, 
            MachineGunAmmo = authoring.machineGunAmmo
        });
    }
}
