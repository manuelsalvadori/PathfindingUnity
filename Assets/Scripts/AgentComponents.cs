using Unity.Entities;
using Unity.Mathematics;

public struct Agent : IComponentData {}

public struct Target : IComponentData
{
    public float3 Value;
}

public struct Step : IComponentData
{
    public int Value;
}

// path components

public struct PathStep : IComponentData
{
    public float3 Value;
}

public struct PathIndex : IComponentData
{
    public int Value;
}

public struct ParentAgent : ISharedComponentData
{
    public Entity Value;
}