using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicProperty
{
    /// 64-bit map: long/double/ticks-backed (long/double/DateTime/TimeSpan/…)
    [Serializable]
    public sealed class PropertySet64 : ISerializationCallbackReceiver
    {
        [SerializeField] private List<DynamicProperty64> _items = new();

        [NonSerialized] private Dictionary<int, int> _index;
        [NonSerialized] private bool _indexBuilt;

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => RebuildIndex();

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

        public bool Contains(int id)
        {
            EnsureIndex();
            return _index.ContainsKey(id);
        }

        // ---- getters ----
        public bool TryGetLong(int id, out long v)
        {
            EnsureIndex();
            v = default;
            if (_index.TryGetValue(id, out var i)) { v = _items[i].GetLong(); return true; }
            return false;
        }

        public bool TryGetDouble(int id, out double v)
        {
            EnsureIndex();
            v = default;
            if (_index.TryGetValue(id, out var i)) { v = _items[i].GetDouble(); return true; }
            return false;
        }

        public bool TryGetDateTimeTicks(int id, out DateTime v)
        {
            EnsureIndex();
            v = default;
            if (_index.TryGetValue(id, out var i)) { v = _items[i].GetDateTimeTicks(); return true; }
            return false;
        }

        public bool TryGetTimeSpanTicks(int id, out TimeSpan v)
        {
            EnsureIndex();
            v = default;
            if (_index.TryGetValue(id, out var i)) { v = _items[i].GetTimeSpanTicks(); return true; }
            return false;
        }

        public bool TryGetEnum32<TEnum>(int id, out TEnum v) where TEnum : struct, Enum
        {
            // If you also store enums in 64 container as 32-bit int masks
            EnsureIndex();
            v = default;
            if (_index.TryGetValue(id, out var i)) { v = _items[i].GetEnum<TEnum>(); return true; }
            return false;
        }

        // ---- setters ----
        public void SetLong(int id, long value)
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i)) { var p = _items[i]; p.SetLong(value); _items[i] = p; }
            else { var p = new DynamicProperty64 { id = id }; p.SetLong(value); _items.Add(p); _index[id] = _items.Count - 1; }
        }

        public void SetDouble(int id, double value)
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i)) { var p = _items[i]; p.SetDouble(value); _items[i] = p; }
            else { var p = new DynamicProperty64 { id = id }; p.SetDouble(value); _items.Add(p); _index[id] = _items.Count - 1; }
        }

        public void SetDateTimeTicks(int id, DateTime value)
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i)) { var p = _items[i]; p.SetDateTimeTicks(value); _items[i] = p; }
            else { var p = new DynamicProperty64 { id = id }; p.SetDateTimeTicks(value); _items.Add(p); _index[id] = _items.Count - 1; }
        }

        public void SetTimeSpanTicks(int id, TimeSpan value)
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i)) { var p = _items[i]; p.SetTimeSpanTicks(value); _items[i] = p; }
            else { var p = new DynamicProperty64 { id = id }; p.SetTimeSpanTicks(value); _items.Add(p); _index[id] = _items.Count - 1; }
        }

        public void SetEnum32<TEnum>(int id, TEnum value) where TEnum : struct, Enum
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i)) { var p = _items[i]; p.SetEnum(value); _items[i] = p; }
            else { var p = new DynamicProperty64 { id = id }; p.SetEnum(value); _items.Add(p); _index[id] = _items.Count - 1; }
        }

        public bool Remove(int id)
        {
            EnsureIndex();
            if (_index.TryGetValue(id, out var i))
            {
                _items.RemoveAt(i);
                _indexBuilt = false;
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

        public IReadOnlyList<DynamicProperty64> RawItems => _items;
    }
}
