using System.Diagnostics.CodeAnalysis;

namespace Finance.Domain.Entities
{
    public class DatedSeries<T> : IDictionary<DateTime, T>
    {
        public IDictionary<DateTime, T> Series { get; }

        public ICollection<DateTime> Keys => Series.Keys;

        public ICollection<T> Values => Series.Values;

        public int Count => Series.Count;

        public bool IsReadOnly => Series.IsReadOnly;

        public T this[DateTime key] { get => Series[key]; set => this[key] = value; }

        public DatedSeries(IDictionary<DateTime, T> series)
        {
            Series = series ?? throw new ArgumentNullException(nameof(series));
        }

        public static DatedSeries<T> FromDictionary(IDictionary<DateTime, T> series)
        {
            return new DatedSeries<T>(series);
        }

        public void Add(DateTime key, T value)
        {
            Series.Add(key, value);
        }

        public bool ContainsKey(DateTime key)
        {
            return Series.ContainsKey(key);
        }

        public bool Remove(DateTime key)
        {
            return Series.Remove(key);
        }

        public bool TryGetValue(DateTime key, [MaybeNullWhen(false)] out T value)
        {
            return Series.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<DateTime, T> item)
        {
            Series.Add(item);
        }

        public void Clear()
        {
            Series?.Clear();
        }

        public bool Contains(KeyValuePair<DateTime, T> item)
        {
            return Series.Contains(item);
        }

        public void CopyTo(KeyValuePair<DateTime, T>[] array, int arrayIndex)
        {
            Series.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<DateTime, T> item)
        {
            return Series.Remove(item);
        }

        public IEnumerator<KeyValuePair<DateTime, T>> GetEnumerator()
        {
            return Series.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Series.GetEnumerator();
        }
    }
}
