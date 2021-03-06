﻿using Unity.Entities;
using Unity.Mathematics;


public struct Node : IComponentData {}

public struct Walkable : IComponentData
{
    public TBool Value;
}

public struct Cost : IComponentData
{
    public int Value;
}

public struct TBool
{
    private readonly byte _value;
    public TBool(bool value) { _value = (byte)(value ? 1 : 0); }
    public static implicit operator TBool(bool value) { return new TBool(value); }
    public static implicit operator bool(TBool value) { return value._value != 0; }
}