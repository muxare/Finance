using Finance.Domain.Entities.ValueObjects;

namespace Finance.Domain.Entities
{
    public class Ema : IProcessor<Series<DateTime, double>, Series<DateTime, double>>
    {
        public int Days { get; }
        public IProcessor<Series<DateTime, double>, Series<DateTime, double>> Processor { get; }
        public string KeyName => $"ema-{Days}";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Ema(int Days)
        {
            this.Days = Days;
        }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Series<DateTime, double> Calculate(Series<DateTime, double> list)
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

            var ema = new Series<DateTime, double>(series);

            return ema;
        }
    }
}
