using Finance.Api.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace Finance.Api.Domain
{
    public class EmaFanSeries : IDictionary<DateTime, EmaFanEntry>
    {
        private IDictionary<DateTime, EmaFanEntry> Series { get; }

        public EmaFanSeries(IDictionary<DateTime, EmaFanEntry> series)
        {
            Series = series ?? throw new ArgumentNullException(nameof(series));
        }

        public EmaFanEntry this[DateTime key] { get => Series[key]; set => Series[key] = value; }

        public ICollection<DateTime> Keys => Series.Keys;

        public ICollection<EmaFanEntry> Values => Series.Values;

        public int Count => Series.Count;

        public bool IsReadOnly => Series.IsReadOnly;

        public void Add(DateTime key, EmaFanEntry value)
        {
            Series.Add(key, value);
        }

        public void Add(KeyValuePair<DateTime, EmaFanEntry> item)
        {
            Series.Add(item);
        }

        public void Clear()
        {
            Series?.Clear();
        }

        public bool Contains(KeyValuePair<DateTime, EmaFanEntry> item)
        {
            return Series.Contains(item);
        }

        public bool ContainsKey(DateTime key)
        {
            return ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<DateTime, EmaFanEntry>[] array, int arrayIndex)
        {
            Series.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<DateTime, EmaFanEntry>> GetEnumerator()
        {
            return Series.GetEnumerator();
        }

        public bool Remove(DateTime key)
        {
            return Series.Remove(key);
        }

        public bool Remove(KeyValuePair<DateTime, EmaFanEntry> item)
        {
            return Series.Remove(item);
        }

        public bool TryGetValue(DateTime key, [MaybeNullWhen(false)] out EmaFanEntry value)
        {
            return Series.TryGetValue(key, out value);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
