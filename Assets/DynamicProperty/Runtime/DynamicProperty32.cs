using System;
using UnityEngine;

namespace DynamicProperty
{
    [Serializable]
    public struct DynamicProperty32
    {
        [SerializeField] public int id;       // game-plugged enum value
        [SerializeField] public int rawValue; // union storage

        public int Id => id;
        public int RawValue => rawValue;
    }
}
