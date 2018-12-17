using Unity.Entities;
using Unity.Mathematics;

public struct Agent : IComponentData {}

public struct Target : IComponentData
{
    public float3 Value;
}

[InternalBufferCapacity(25)]
public struct Waypoints : IBufferElementData
{
    public int2 Value;
}