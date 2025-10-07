using System;
using UnityEngine;

[Serializable]
public struct Property64
{
    [SerializeField] private PropertyKey key;
    [SerializeField] private long rawValue;

    public PropertyKey Key => key;
    public long RawValue { get => rawValue; set => rawValue = value; }
}
