using System.Text.Json.Serialization;

namespace Finance.Api.Domain.ValueObjects
{
    public class Open : ValueObject
    {
        //[JsonPropertyName("O")]
        public double? OpenCore { get; }

        [JsonConstructor]
        public Open(double? openCore)
        {
            OpenCore = openCore;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return OpenCore;
        }

        public static implicit operator Open(double d) => new Open(d);

        public static implicit operator Open(string s)
        {
            if (s == "null")
                return new Open(null);
            return new Open(double.Parse(s));
        }
    }
}
