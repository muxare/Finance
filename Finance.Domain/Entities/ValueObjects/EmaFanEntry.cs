using Finance.Domain.Common;

namespace Finance.Api.Domain.ValueObjects
{
    public class EmaFanEntry : ValueObject
    {
        public DateTime DateTime { get; set; }
        public double Value18 { get; set; }
        public double Value50 { get; set; }
        public double Value100 { get; set; }
        public double Value200 { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DateTime;
            yield return Value18;
            yield return Value50;
            yield return Value100;
            yield return Value200;
        }
    }

    public class TrendEntry : ValueObject
    {
        public DateTime DateTime { get; set; }

        public TrendType TrendType { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DateTime;
            yield return TrendType;
        }
    }

    public enum TrendType
    {
        Upp,
        Between,
        Down
    }
}
