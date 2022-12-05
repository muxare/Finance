using Finance.Api.Domain.ValueObjects;

namespace Finance.Api.Domain
{
    public class Ema : IProcessor<Series<DateTime, double>, Series<DateTime, double>>
    {
        public int Days { get; }
        public IProcessor<Series<DateTime, double>, Series<DateTime, double>> Processor { get; }
        public string KeyName => $"ema-{Days}";

        public Ema(int Days)
        {
            this.Days = Days;
        }

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
