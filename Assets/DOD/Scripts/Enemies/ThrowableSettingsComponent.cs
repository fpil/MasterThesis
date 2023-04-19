using Unity.Entities;
using Unity.Mathematics;

public struct ThrowableSettingsComponent : IComponentData
{
    public float3 startPos;
    public float3 targetPos;
    public float distance;
    public double startTime;
    public float height;
    public float terrainHeight;
}