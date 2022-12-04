namespace Finance.Api.Domain.Tests
{
    public class DatedSeriesTests
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            var seed = new List<DateTime> { DateTime.Parse("2022-01-01") };
            var dates = Enumerable.Range(1, 10).Aggregate<int, List<DateTime>>(seed: seed, (agg, next) =>
            {
                agg.Add(agg.Last().NextWeekday());
                return agg;
            });
            var values = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 2 };
            var zipped = dates.Zip(values).ToDictionary(o => o.First, o => o.Second);
            var series = new DatedSeries<int>(zipped);

            // Act

            // Assert
        }
    }

    public static class DateTimeExtentions
    {
        public static DateTime NextWeekday(this DateTime d)
        {
            int i = 1;
            if (d.DayOfWeek == DayOfWeek.Friday)
            {
                i = 3;
            }
            if (d.DayOfWeek == DayOfWeek.Saturday)
            {
                i = 3;
            }
            return d.AddDays(i);
        }
    }
}
