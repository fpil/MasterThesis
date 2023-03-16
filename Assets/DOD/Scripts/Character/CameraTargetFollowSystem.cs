using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial class CameraTargetFollowSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<PlayerTagComponent>().ForEach((ref LocalTransform localTransform) =>
        {
            FollowPlayer.Instance.UpdateTargetPosition(localTransform.Position);
            FollowPlayer.Instance.UpdateTargetRotation(localTransform.Rotation);

        }).WithoutBurst().Run();
    }
}
