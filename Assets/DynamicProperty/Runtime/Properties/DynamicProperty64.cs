using System;
using UnityEngine;

namespace DynamicProperty
{
    [Serializable]
    public struct DynamicProperty64
    {
        [SerializeField] int id;       // game-plugged enum value
        [SerializeField] long rawValue; // union storage

        public DynamicProperty64(int id) : this()
        {
            this.id = id;
        }

        public readonly int Id => id;
        public long RawValue { readonly get => rawValue; set => rawValue = value; }
    }
}
