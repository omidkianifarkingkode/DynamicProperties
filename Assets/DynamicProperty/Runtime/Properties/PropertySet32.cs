using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicProperty
{
    /// 32-bit map: int-backed values (int/float/bool/enum/…)
    [Serializable]
    public sealed class PropertySet32 : ISerializationCallbackReceiver
    {
        [SerializeField] private List<DynamicProperty32> _items = new();

        [NonSerialized] private Dictionary<int, int> _index; // id -> list index
        [NonSerialized] private bool _indexBuilt;

        // ---------- Unity serialization hooks ----------
        public void OnBeforeSerialize() { /* nothing */ }
        public void OnAfterDeserialize()
        {
            // Called by Unity whenever this object is deserialized (loading assets/scenes, domain reload, entering play, etc.)
            RebuildIndex();
        }

        // Optional: call from owner ScriptableObject.OnEnable() for extra safety
        public void RebuildIndex()
        {
            _index ??= new Dictionary<int, int>(_items.Count);
            _index.Clear();
            for (int i = 0; i < _items.Count; i++)
            {
                int id = _items[i].Id;
                if (!_index.ContainsKey(id)) _index.Add(id, i);
            }
            _indexBuilt = true;
        }

        private void EnsureIndex()
        {
            if (!_indexBuilt || _index == null) RebuildIndex();
        }

        // ---------- Presence ----------
        public bool Contains(int id)
        {
            EnsureIndex();
            return _index.ContainsKey(id);
        }

        // ---------- Getters ----------
        public bool TryGetInt(int id, out int v)
        {
            EnsureIndex();
            v = default;
            if (_index.TryGetValue(id, out var i)) { v = _items[i].GetInt(); return true; }
            return false;
        }

        public bool TryGetFloat(int id, out float v)
        {
            EnsureIndex();
            v = default;
            if (_index.TryGetValue(id, out var i)) { v = _items[i].GetFloat(); return true; }
            return false;
        }

        public bool TryGetBool(int id, out bool v)
        {
            EnsureIndex();
            v = default;
            if (_index.TryGetValue(id, out var i)) { v = _items[i].GetBool(); return true; }
            return false;
        }

        public bool TryGetEnum<TEnum>(int id, out TEnum v) where TEnum : struct, Enum
        {
            EnsureIndex();
            v = default;
            if (_index.TryGetValue(id, out var i)) { v = _items[i].GetEnum<TEnum>(); return true; }
            return false;
        }

        // ---------- Setters (update index automatically) ----------
        public void SetInt(int id, int value)
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i))
            {
                var p = _items[i]; p.SetInt(value); _items[i] = p;
            }
            else
            {
                var p = new DynamicProperty32 { id = id }; p.SetInt(value);
                _items.Add(p);
                _index[id] = _items.Count - 1;
            }
        }

        public void SetFloat(int id, float value)
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i))
            {
                var p = _items[i]; p.SetFloat(value); _items[i] = p;
            }
            else
            {
                var p = new DynamicProperty32 { id = id }; p.SetFloat(value);
                _items.Add(p);
                _index[id] = _items.Count - 1;
            }
        }

        public void SetBool(int id, bool value)
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i))
            {
                var p = _items[i]; p.SetBool(value); _items[i] = p;
            }
            else
            {
                var p = new DynamicProperty32 { id = id }; p.SetBool(value);
                _items.Add(p);
                _index[id] = _items.Count - 1;
            }
        }

        public void SetEnum<TEnum>(int id, TEnum value) where TEnum : struct, Enum
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i))
            {
                var p = _items[i]; p.SetEnum(value); _items[i] = p;
            }
            else
            {
                var p = new DynamicProperty32 { id = id }; p.SetEnum(value);
                _items.Add(p);
                _index[id] = _items.Count - 1;
            }
        }

        // ---------- Remove / Clear ----------
        public bool Remove(int id)
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i))
            {
                _items.RemoveAt(i);
                _indexBuilt = false; // indices shifted; rebuild on next access
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _items.Clear();
            _index?.Clear();
            _indexBuilt = true;
        }

        // ---------- Optional: expose read-only view ----------
        public IReadOnlyList<DynamicProperty32> RawItems => _items;
    }
}
