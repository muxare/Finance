using System.Text.Json.Serialization;

namespace Finance.Api.Domain.ValueObjects
{
    public class Low : ValueObject
    {
        //[JsonPropertyName("L")]
        public double? LowCore { get; }

        [JsonConstructor]
        public Low(double? lowCore)
        {
            LowCore = lowCore;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return LowCore;
        }

        public static implicit operator Low(double d) => new Low(d);

        public static implicit operator Low(string s)
        {
            if (s == "null")
                return new Low(null);
            return new Low(double.Parse(s));
        }
    }
}
