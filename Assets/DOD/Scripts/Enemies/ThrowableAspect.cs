using Assets.DOD.Scripts.Enemies;
using DOD.Scripts.Enemies;
using Unity.Entities;
using Unity.Mathematics;

public readonly partial struct ThrowableAspect : IAspect
{
    readonly RefRO<ThrowableComponent> m_ThrowablePrefab;

    // Note the use of ValueRO in the following properties.
    public Entity ThrowablePrefab => m_ThrowablePrefab.ValueRO.ThrowablePrefab;
}
