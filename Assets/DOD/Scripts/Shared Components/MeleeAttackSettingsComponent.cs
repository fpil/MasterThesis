using Unity.Entities;

public struct MeleeAttackSettingsComponent : ISharedComponentData
{
    public float Range;
    public float MaxTimer;
}