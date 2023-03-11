namespace Finance.Domain.Entities
{
    public interface IProcessor<TSource, TIndicator>
    {
        TIndicator Calculate(TSource list);

        string KeyName { get; }
    }
}
