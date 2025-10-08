using System.Runtime.InteropServices;

namespace DynamicProperty
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ValueUnion32
    {
        [FieldOffset(0)] public int raw;
        [FieldOffset(0)] public int asInt;
        [FieldOffset(0)] public float asFloat;
        [FieldOffset(0)] public bool asBool;
    }
}
