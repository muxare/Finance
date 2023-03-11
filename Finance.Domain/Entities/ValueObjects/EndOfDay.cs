using System.Text.Json.Serialization;

namespace Finance.Api.Domain.ValueObjects
{
    public class EndOfDay : ValueObject
    {
        public DateTime DateTime { get; }
        public Open Open { get; }
        public High High { get; }
        public Low Low { get; }
        public Close Close { get; }
        public AdjClose AdjClose { get; }
        public Volume Volume { get; }

        /*public EndOfDay(DateTime dateTime, double open, double high, double low, double close, double adjClose, int volume)
        {
            DateTime = dateTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            AdjClose = adjClose;
            Volume = volume;
        }*/

        [JsonConstructor]
        public EndOfDay(DateTime dateTime, Open open, High high, Low low, Close close, AdjClose adjClose, Volume volume)
        {
            DateTime = dateTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            AdjClose = adjClose;
            Volume = volume;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DateTime;
            yield return Open;
            yield return High;
            yield return Low;
            yield return Close;
            yield return AdjClose;
            yield return Volume;
        }

        public static implicit operator string(EndOfDay endOfDay)
        {
            return $"{endOfDay.DateTime},{endOfDay.Open.OpenCore},{endOfDay.High.HighCore},{endOfDay.Low.LowCore},{endOfDay.Close.CloseCore},{endOfDay.AdjClose.AdjCloseCore},{endOfDay.Volume.VolumeCore}";
        }

        public static implicit operator EndOfDay(string s)
        {
            var cells = s.Split(',');
            if (cells.Length != 7)
            {
                return null;
            }
            return new EndOfDay(
                DateTime.Parse(cells[0]),
                cells[1],
                cells[2],
                cells[3],
                cells[4],
                cells[5],
                cells[6]);
        }
    }
}
