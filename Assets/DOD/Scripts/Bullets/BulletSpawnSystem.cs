using DOD.Scripts.Bullets;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;


[BurstCompile]
public partial struct BulletSpawnSystem : ISystem
{
    private Random generator;
    private float lastAttack;

    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        lastAttack+= state.WorldUnmanaged.Time.DeltaTime;
        generator = new Random((uint) UnityEngine.Random.Range(-10000, 10000));
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var muzzleGameObject = GameObject.Find("MuzzleGameObject");

        if (Input.GetButtonDown("Fire1"))
        {
            if (lastAttack >= 0.3f)
            {
                if (muzzleGameObject.tag == "Handgun")
                {
                    var bulletSpawnJob = new SpawnBulletJob
                    {
                        Ecb = ecb,
                        muzzleGameObjectPosition = muzzleGameObject.transform.position,
                        muzzleGameObjectRotation = muzzleGameObject.transform.rotation,
                        WeaponType = 0
                    };
                    // Schedule execution in a single thread, and do not block main thread.
                    bulletSpawnJob.Run();
                    lastAttack = 0;
                }
            }
            if (lastAttack >= 0.5f)
            {
                if (muzzleGameObject.tag == "Shotgun")
                {
                    var bulletSpawnJob = new SpawnBulletJob
                    {
                        Ecb = ecb,
                        muzzleGameObjectPosition = muzzleGameObject.transform.position,
                        muzzleGameObjectRotation = muzzleGameObject.transform.rotation,
                        WeaponType = 1,
                        generator = generator

                    };
                    // Schedule execution in a single thread, and do not block main thread.
                    bulletSpawnJob.Run();
                    lastAttack = 0;
                }
            }
        }
        else if (Input.GetButton("Fire1"))
        {
            if (lastAttack >= 0.1f)
            {
                if (muzzleGameObject.tag == "Machinegun")
                {
                    var bulletSpawnJob = new SpawnBulletJob
                    {
                        Ecb = ecb,
                        muzzleGameObjectPosition = muzzleGameObject.transform.position,
                        muzzleGameObjectRotation = muzzleGameObject.transform.rotation,
                        WeaponType = 2,
                        generator = generator
                    };
                    // Schedule execution in a single thread, and do not block main thread.
                    bulletSpawnJob.Run();
                    lastAttack = 0;
                }
            }
        }
    }

    [BurstCompile]
    public partial struct SpawnBulletJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;
        public int WeaponType { get; set; }
        public Vector3 muzzleGameObjectPosition { get; set; }
        public Quaternion muzzleGameObjectRotation { get; set; }
        public Random generator;


        void Execute(ref BulletSpawnAspect bulletSpawnAspect)
        {
            //Handgun
            if (WeaponType == 0)
            {
                var instance = Ecb.Instantiate(bulletSpawnAspect.BulletPrefab);
                var bulletTransform = LocalTransform.FromPosition(muzzleGameObjectPosition);
                bulletTransform.Rotation = muzzleGameObjectRotation;
                AddComponents(instance, bulletTransform, 50);
            }
            //Shotgun
            else if (WeaponType == 1)
            {
                if (bulletSpawnAspect.ShotgunAmmo > 0)
                {
                    float spread = 2.0f;
                    for (int i = 0; i < 6; i++) // spawn 5 bullets
                    {
                        Quaternion spreadRotation = Quaternion.Euler(generator.NextFloat(-spread, spread), generator.NextFloat(-spread, spread), 0f);
                        var instance = Ecb.Instantiate(bulletSpawnAspect.BulletPrefab);
                        var bulletTransform = LocalTransform.FromPosition(muzzleGameObjectPosition);
                        bulletTransform.Rotation = spreadRotation*muzzleGameObjectRotation;
                        AddComponents(instance, bulletTransform, 40);
                    }
                    bulletSpawnAspect.ShotgunAmmo--;
                }
            }
            //Machinegun
            else if (WeaponType == 2)
            {
                if (bulletSpawnAspect.MachineGunAmmo > 0)
                {
                    float spread = 2.0f;
                    Quaternion spreadRotation = Quaternion.Euler(generator.NextFloat(-spread, spread), generator.NextFloat(-spread, spread), 0f);
                    var instance = Ecb.Instantiate(bulletSpawnAspect.BulletPrefab);
                    var bulletTransform = LocalTransform.FromPosition(muzzleGameObjectPosition);
                    bulletTransform.Rotation = spreadRotation*muzzleGameObjectRotation;
                    AddComponents(instance, bulletTransform,60);
                    bulletSpawnAspect.MachineGunAmmo--;
                }
            }
        }

        void AddComponents(Entity instance, LocalTransform transform, float speed)
        {
            Ecb.SetComponent(instance, transform);
            Ecb.SetComponent(instance, new BulletFired
            {
                _hasFired = 0
            });
            Ecb.SetComponent(instance, new BulletLifeTime
            {
                maxLifeTime = 2
            });
            Ecb.AddComponent(instance, new SpeedComponent
            {
                Value = speed
            });
            Ecb.AddComponent<IsDeadComponent>(instance);
            Ecb.SetComponentEnabled(instance,ComponentType.ReadWrite<IsDeadComponent>(), false);

        }
    }
}
