using System;

namespace DynamicProperty
{
    [Flags]
    public enum PropertyBitness
    {
        None = 0,
        Bit32 = 1 << 0,
        Bit64 = 1 << 1,
        Both = Bit32 | Bit64
    }
}
