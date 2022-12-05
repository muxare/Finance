using Finance.Api.Domain.ValueObjects;

namespace Finance.Api.Domain
{
    public static class EmaFanSeriesExtentions
    {
        public static EmaFanSeries ToEmaFanSeries(this IEnumerable<EmaFanEntry> source)
        {
            return new EmaFanSeries(source.ToDictionary(o => o.DateTime, o => o));
        }

        public static DatedSeries<bool> UpTrend(this EmaFanSeries series)
        {
            return new DatedSeries<bool>(series.ToDictionary(k => k.Key, v => v.Value.Value18 > v.Value.Value50 && v.Value.Value50 > v.Value.Value100 && v.Value.Value100 > v.Value.Value200));
        }

    }
}
