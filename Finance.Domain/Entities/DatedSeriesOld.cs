namespace Finance.Domain.Entities
{
    public class DatedSeriesOld<T>
    {
        public DateTime DateTime { get; set; }
        public T? Value { get; set; }
    }
}
