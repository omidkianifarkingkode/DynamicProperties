using System;
using UnityEngine;

namespace DynamicProperty
{
    [Serializable]
    public struct DynamicProperty64
    {
        [SerializeField] public int id;       // game-plugged enum value
        [SerializeField] public long rawValue; // union storage

        public int Id => id;
        public long RawValue { get => rawValue; set => rawValue = value; }
    }
}
