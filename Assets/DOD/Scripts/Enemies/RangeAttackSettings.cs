using Unity.Entities;

public struct RangeAttackSettingsComponent : ISharedComponentData
{
    public float Range;
    public float MaxTimer;
}