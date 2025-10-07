using System;

[Serializable]
public struct Property32
{
    public PropertyKey key;
    public int rawValue;

    public PropertyKey Key => key;
    public int RawValue => rawValue;
}
