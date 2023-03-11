using System.Text.Json.Serialization;
using Finance.Domain.Common;

namespace Finance.Api.Domain.ValueObjects
{
    public class High : ValueObject
    {
        //[JsonPropertyName("H")]
        public double? HighCore { get; }

        [JsonConstructor]
        public High(double? highCore)
        {
            HighCore = highCore;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HighCore;
        }

        public static implicit operator High(double d) => new High(d);

        public static implicit operator High(string s)
        {
            if (s == "null")
                return new High(null);
            return new High(double.Parse(s));
        }
    }
}
