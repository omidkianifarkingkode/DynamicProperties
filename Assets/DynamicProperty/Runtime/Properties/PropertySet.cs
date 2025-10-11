using DynamicProperty;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DynamicProperty
{
    /// A unified set that stores 32- and 64-bit properties in two serialized lists,
    /// but exposes a single API + O(1) lookups.
    [Serializable]
    public sealed class PropertySet : ISerializationCallbackReceiver
    {
        [SerializeField] private List<DynamicProperty32> _items32 = new();
        [SerializeField] private List<DynamicProperty64> _items64 = new();

        [NonSerialized] private Dictionary<int, int> _index32;
        [NonSerialized] private Dictionary<int, int> _index64;
        [NonSerialized] private bool _indexed;

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => RebuildIndex();

        public void RebuildIndex()
        {
            _index32 = new Dictionary<int, int>(_items32.Count);
            _index64 = new Dictionary<int, int>(_items64.Count);
            for (int i = 0; i < _items32.Count; i++) _index32[_items32[i].Id] = i;
            for (int i = 0; i < _items64.Count; i++) _index64[_items64[i].Id] = i;
            _indexed = true;
        }
        private void EnsureIndex()
        {
            if (!_indexed || _index32 == null || _index64 == null) RebuildIndex();
        }

        // ---------------- Presence ----------------
        public bool Contains32(int id) { EnsureIndex(); return _index32.ContainsKey(id); }
        public bool Contains64(int id) { EnsureIndex(); return _index64.ContainsKey(id); }
        public bool ContainsAny(int id) { EnsureIndex(); return _index32.ContainsKey(id) || _index64.ContainsKey(id); }

        // ---------------- Getters (32) ----------------
        public bool TryGetInt(int id, out int v)
        {
            EnsureIndex(); v = default;
            if (_index32.TryGetValue(id, out var i)) { v = _items32[i].GetInt(); return true; }
            return false;
        }
        public bool TryGetFloat(int id, out float v)
        {
            EnsureIndex(); v = default;
            if (_index32.TryGetValue(id, out var i)) { v = _items32[i].GetFloat(); return true; }
            return false;
        }
        public bool TryGetBool(int id, out bool v)
        {
            EnsureIndex(); v = default;
            if (_index32.TryGetValue(id, out var i)) { v = _items32[i].GetBool(); return true; }
            return false;
        }
        public bool TryGetEnum32<TEnum>(int id, out TEnum v) where TEnum : struct, Enum
        {
            EnsureIndex(); v = default;
            if (_index32.TryGetValue(id, out var i)) { v = _items32[i].GetEnum<TEnum>(); return true; }
            return false;
        }

        // ---------------- Getters (64) ----------------
        public bool TryGetLong(int id, out long v)
        {
            EnsureIndex(); v = default;
            if (_index64.TryGetValue(id, out var i)) { v = _items64[i].GetLong(); return true; }
            return false;
        }
        public bool TryGetDouble(int id, out double v)
        {
            EnsureIndex(); v = default;
            if (_index64.TryGetValue(id, out var i)) { v = _items64[i].GetDouble(); return true; }
            return false;
        }
        public bool TryGetDateTime(int id, out DateTime v)
        {
            EnsureIndex(); v = default;
            if (_index64.TryGetValue(id, out var i)) { v = _items64[i].GetDateTimeTicks(); return true; }
            return false;
        }
        public bool TryGetTimeSpan(int id, out TimeSpan v)
        {
            EnsureIndex(); v = default;
            if (_index64.TryGetValue(id, out var i)) { v = _items64[i].GetTimeSpanTicks(); return true; }
            return false;
        }
        public bool TryGetEnum64<TEnum>(int id, out TEnum v) where TEnum : struct, Enum
        {
            EnsureIndex(); v = default;
            if (_index64.TryGetValue(id, out var i)) { v = _items64[i].GetEnum<TEnum>(); return true; }
            return false;
        }

        // ---------------- Setters (auto-route by bitness) ----------------
        // Provide both: explicit container setters AND auto by metadata.
        public void SetInt(int id, int value) => UpsertIn32(id, p => p.SetInt(value));
        public void SetFloat(int id, float value) => UpsertIn32(id, p => p.SetFloat(value));
        public void SetBool(int id, bool value) => UpsertIn32(id, p => p.SetBool(value));
        public void SetEnum32<TEnum>(int id, TEnum value) where TEnum : struct, Enum => UpsertIn32(id, p => p.SetEnum(value));

        public void SetLong(int id, long value) => UpsertIn64(id, p => p.SetLong(value));
        public void SetDouble(int id, double value) => UpsertIn64(id, p => p.SetDouble(value));
        public void SetDateTime(int id, DateTime value) => UpsertIn64(id, p => p.SetDateTimeTicks(value));
        public void SetTimeSpan(int id, TimeSpan value) => UpsertIn64(id, p => p.SetTimeSpanTicks(value));
        public void SetEnum64<TEnum>(int id, TEnum value) where TEnum : struct, Enum => UpsertIn64(id, p => p.SetEnum(value));

        // ---------------- Removes ----------------
        public bool Remove(int id)
        {
            EnsureIndex();
            bool removed = false;
            if (_index32.TryGetValue(id, out var i))
            {
                _items32.RemoveAt(i); removed = true;
            }
            if (_index64.TryGetValue(id, out var j))
            {
                _items64.RemoveAt(j); removed = true;
            }
            if (removed) _indexed = false;
            return removed;
        }

        public void Clear()
        {
            _items32.Clear(); _items64.Clear();
            _index32?.Clear(); _index64?.Clear();
            _indexed = true;
        }

        // ---------------- Raw views (optional) ----------------
        public IReadOnlyList<DynamicProperty32> Raw32 => _items32;
        public IReadOnlyList<DynamicProperty64> Raw64 => _items64;

        // ---------------- internals ----------------
        private void UpsertIn32(int id, Action<DynamicProperty32> setter)
        {
            EnsureIndex();
            if (_index32.TryGetValue(id, out var i))
            {
                var p = _items32[i]; setter(p); _items32[i] = p;
            }
            else
            {
                var p = new DynamicProperty32 { id = id };
                setter(p);
                _items32.Add(p);
                _index32[id] = _items32.Count - 1;
            }
        }
        private void UpsertIn64(int id, Action<DynamicProperty64> setter)
        {
            EnsureIndex();
            if (_index64.TryGetValue(id, out var i))
            {
                var p = _items64[i]; setter(p); _items64[i] = p;
            }
            else
            {
                var p = new DynamicProperty64 { id = id };
                setter(p);
                _items64.Add(p);
                _index64[id] = _items64.Count - 1;
            }
        }
    }
}
