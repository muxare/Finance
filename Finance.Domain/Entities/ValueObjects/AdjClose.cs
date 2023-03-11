using System.Text.Json.Serialization;

namespace Finance.Api.Domain.ValueObjects
{
    public class AdjClose : ValueObject
    {
        //[JsonPropertyName("AC")]
        public double? AdjCloseCore { get; }

        [JsonConstructor]
        public AdjClose(double? adjCloseCore)
        {
            AdjCloseCore = adjCloseCore;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return AdjCloseCore;
        }

        public static implicit operator AdjClose(double d) => new(d);

        public static implicit operator AdjClose(string s)
        {
            if (s == "null")
                return new AdjClose(null);
            return new AdjClose(double.Parse(s));
        }
    }
}
