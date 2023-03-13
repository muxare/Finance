using Finance.Domain.Entities;
using Finance.Domain.Entities.ValueObjects;

namespace Finance.Application.Processors
{
    public class Ema : Series<DateTime, double>
    {
        public Ema(IDictionary<DateTime, double> seriesCore) : base(seriesCore)
        {
        }
    }

    public class EmaProcessor : IProcessor<Series<DateTime, double>, Ema>
    {
        public int Days { get; }
        public IProcessor<Series<DateTime, double>, Ema> Processor { get; }
        public string KeyName => $"ema-{Days}";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public EmaProcessor(int Days)
        {
            this.Days = Days;
        }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Ema Calculate(Series<DateTime, double> list)
        {
            var smoothing = 2.0;
            var previous = 0.0;
            var beta = smoothing / (1 + Days);
            var series = list.Select((d, i) =>
            {
                var current = d.Value * beta + previous * (1.0 - beta);
                previous = current;
                return new { DateTime = d.Key, Value = current };
            }).ToDictionary(o => o.DateTime, o => o.Value);

            var ema = new Ema(series);

            return ema;
        }
    }

    public class Macd : Series<DateTime, double>
    {
        public Series<DateTime, double> Signal { get; set; }

        public Macd(Series<DateTime, double> seriesCore) : base(seriesCore)
        {
            Signal = (new EmaProcessor(9)).Calculate(seriesCore);
        }
    }

    public class MacdProcessor : IProcessor<Series<DateTime, double>, Macd>
    {
        public int longSignal { get; set; } = 24;
        public int shortSignal { get; set; } = 12;
        public string KeyName => $"macd-{longSignal}-{shortSignal}";

        public Macd Calculate(Series<DateTime, double> list)
        {
            var emaShort = new EmaProcessor(shortSignal);
            var emaLong = new EmaProcessor(longSignal);

            Series<DateTime, double> macd = emaShort.Calculate(list) - emaLong.Calculate(list);

            return new Macd(macd);
        }
    }
}
