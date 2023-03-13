using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Finance.Domain.Common;

namespace Finance.Domain.Entities.ValueObjects
{
    public class Series<T> : ValueObject
    {
        public IEnumerable<T> SeriesCore { get; }

        [JsonConstructor]
        public Series(IEnumerable<T> seriesCore)
        {
            SeriesCore = seriesCore ?? throw new ArgumentNullException(nameof(seriesCore));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            foreach (T v in SeriesCore)
            {
                yield return v;
            };
        }
    }

    public class Series<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        public IDictionary<TKey, TValue> SeriesCore { get; }

        [JsonConstructor]
        public Series(IDictionary<TKey, TValue> seriesCore)
        {
            SeriesCore = seriesCore ?? new Dictionary<TKey, TValue>();
        }

        public ICollection<TKey> Keys => SeriesCore.Keys;

        public ICollection<TValue> Values => SeriesCore.Values;

        public int Count => SeriesCore.Count;

        public bool IsReadOnly => SeriesCore.IsReadOnly;

        public TValue this[TKey key]
        {
            get => SeriesCore[key];
            set => SeriesCore[key] = value;
        }

        public void Add(TKey key, TValue value)
        {
            SeriesCore.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return SeriesCore.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return SeriesCore.Remove(key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return SeriesCore.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            SeriesCore.Add(item);
        }

        public void Clear()
        {
            SeriesCore.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return SeriesCore.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            SeriesCore.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return SeriesCore.Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return SeriesCore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        //public TValue this[TKey key] => SeriesCore[key];

        //public IEnumerable<TKey> Keys => SeriesCore.Keys;

        //public IEnumerable<TValue> Values => SeriesCore.Values;

        //public int Count => SeriesCore.Count;

        //public bool ContainsKey(TKey key) => SeriesCore.ContainsKey(key);

        //public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => SeriesCore.GetEnumerator();

        //public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => SeriesCore.TryGetValue(key, out value);

        //IEnumerator IEnumerable.GetEnumerator() => SeriesCore.GetEnumerator();
    }
}
