namespace Finance.Api.Domain
{
    public class DatedSeriesOld<T>
    {
        public DateTime DateTime { get; set; }
        public T? Value { get; set; }
    }
}
