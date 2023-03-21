using Assets.DOD.Scripts.Bullets;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace DOD.Scripts.Bullets
{
    [UpdateAfter(typeof(BulletSpawnSystem))]
    public partial struct BulletBehaviourSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = state.WorldUnmanaged.Time.DeltaTime;
            
            //Todo --> this does not really work because the job is never stalled
            var muzzleGameObject = GameObject.Find("MuzzleGameObject");
            var vector3 = muzzleGameObject.transform.forward;


            var bulletFireJob = new BulletFireJob()
            {
                deltaTime = deltaTime,
                vector3 = vector3
            };    
            //Need to be paralle or race condition
            state.Dependency = bulletFireJob.ScheduleParallel(state.Dependency);

            // bulletFireJob.Run();
        }
        
        // [UpdateAfter(typeof(BulletSpawnSystem))]
        [WithAll(typeof(BulletTag))]
        public partial struct BulletFireJob : IJobEntity
        {
            public float deltaTime;
            public Vector3 vector3 { get; set; }

            public void Execute(ref LocalTransform localTransform, ref PhysicsVelocity velocity)
            {
                //This somewhat works
                //Todo --> adjust the physics ask Mathias about this
                velocity.Linear += new float3(vector3 * 20)*deltaTime;
            }
        }
       
    }
}
    
    

