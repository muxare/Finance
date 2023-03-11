using System.Text.Json.Serialization;
using Finance.Domain.Common;

namespace Finance.Api.Domain.ValueObjects
{
    public class Close : ValueObject
    {
        //[JsonPropertyName("C")]
        public double? CloseCore { get; }

        [JsonConstructor]
        public Close(double? closeCore)
        {
            CloseCore = closeCore;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CloseCore;
        }

        public static implicit operator Close(double d) => new Close(d);

        public static implicit operator Close(string s)
        {
            if (s == "null")
                return new Close(null);
            return new Close(double.Parse(s));
        }
    }
}
