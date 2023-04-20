using Unity.Entities;

public struct EntityInUseComponent : IComponentData, IEnableableComponent
{
    public bool inUse;
}