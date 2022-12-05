namespace Finance.Api.Domain
{
    public interface IProcessor<TSource, TIndicator>
    {
        TIndicator Calculate(TSource list);

        string KeyName { get; }
    }
}
