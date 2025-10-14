using System;
using UnityEngine;

namespace DynamicProperty
{
    [Serializable]
    public struct DynamicProperty32
    {
        [SerializeField] int id;       // game-plugged enum value
        [SerializeField] int rawValue; // union storage

        public DynamicProperty32(int id) : this()
        {
            this.id = id;
        }

        public readonly int Id => id;
        public int RawValue { readonly get => rawValue; set => rawValue = value; }
    }
}
