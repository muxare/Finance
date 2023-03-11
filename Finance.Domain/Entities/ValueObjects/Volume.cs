using System.Text.Json.Serialization;
using Finance.Domain.Common;

namespace Finance.Api.Domain.ValueObjects
{
    public class Volume : ValueObject
    {
        // [JsonPropertyName("V")]
        public int? VolumeCore { get; }

        [JsonConstructor]
        public Volume(int? volumeCore)
        {
            VolumeCore = volumeCore;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return VolumeCore;
        }

        public static implicit operator Volume(int volume) => new Volume(volume);

        public static implicit operator Volume(string s)
        {
            if (s == "null")
                return new Volume(null);
            return new Volume(int.Parse(s));
        }
    }
}
