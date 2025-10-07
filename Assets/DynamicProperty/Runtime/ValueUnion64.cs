using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct ValueUnion64
{
    [FieldOffset(0)] public long raw;
    [FieldOffset(0)] public int asInt;
    [FieldOffset(0)] public float asFloat;
    [FieldOffset(0)] public bool asBool;
    [FieldOffset(0)] public long asLong;
    [FieldOffset(0)] public double asDouble;
}


[StructLayout(LayoutKind.Explicit)]
public struct ValueUnion32
{
    [FieldOffset(0)] public int raw;
    [FieldOffset(0)] public int asInt;
    [FieldOffset(0)] public float asFloat;
    [FieldOffset(0)] public bool asBool;
}


