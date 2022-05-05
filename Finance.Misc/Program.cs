// See https://aka.ms/new-console-template for more information
using Domain;
using Dto;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

Console.WriteLine("Hello, World!");

var quotes = File.ReadAllLines("C:\\source\\repos\\Data\\Stocks\\aapl.csv");
var series = quotes.Skip(1).Select(line =>
{
    var l = line.Split(',');
    return new QuoteDtoYahoo(DateTime.Parse(l[0], CultureInfo.InvariantCulture), double.Parse(l[1]), double.Parse(l[2]), double.Parse(l[3]), double.Parse(l[4]), double.Parse(l[5]), long.Parse(l[6]));
}).ToDictionary(o => o.Date, o => o);
var lastQuote = series.LastOrDefault();

/// <summary>
/// Currently the most accurate
/// </summary>

Domain.DatedSeriesNew<double> EmaInvestiopedia(IDictionary<DateTime, Domain.DatedSeries<double>> list, int days)
{
    var smoothing = 2.0;
    var previous = 0.0;
    var beta = smoothing / (1 + days);
    var series = list.Select((d, i) =>
    {
        var current = d.Value.Value * beta + previous * (1.0 - beta);
        previous = current;
        return new { DateTime = d.Key, Value = current };
    }).ToDictionary(o => o.DateTime, o => o.Value);

    var ema = new Domain.DatedSeriesNew<double>(series);

    return ema;
}


/*
Domain.DatedSeriesNew<PriceAction> PriceActionSeries()
{

}
*/



var seriesAsc = series.OrderBy(q => q.Key);
var seriesDesc = series.OrderByDescending(q => q.Key);

var closeSeries = seriesAsc.Select(q => new Domain.DatedSeries<double> { DateTime = q.Value.Date, Value = q.Value.Close }).ToDictionary(o => o.DateTime, o => o);
var ema18 = EmaInvestiopedia(closeSeries, 18);
var ema50 = EmaInvestiopedia(closeSeries, 50);
var ema100 = EmaInvestiopedia(closeSeries, 100);
var ema200 = EmaInvestiopedia(closeSeries, 200);

EmaFanSeries list = closeSeries.Select(q => new Domain.EmaFanEntry
{
    DateTime = q.Key,
    Value18 = ema18[q.Key],
    Value50 = ema50[q.Key],
    Value100 = ema100[q.Key],
    Value200 = ema200[q.Key]
}).ToEmaFanSeries();

State state = new State();
//state = Reducers.Reducer(state, null); 


var resUpTrend = BackTestUpTrend(seriesAsc.ToDictionary(o => o.Key, o => o), ema18, ema50, ema100, ema200);

DatedSeriesNew<bool> BackTestUpTrend(Dictionary<DateTime, KeyValuePair<DateTime, QuoteDtoYahoo>> dictionary, DatedSeriesNew<double> ema18, DatedSeriesNew<double> ema50, DatedSeriesNew<double> ema100, DatedSeriesNew<double> ema200)
{ 
    var res = new Dictionary<DateTime, bool>();

    // Determine the earliest date to back test from 
    var dateStart = dictionary.First().Key;

    // Loop through data and find interesting dates
    bool upTrend;
    bool upTrendPrev = false;
    foreach (DateTime date in dictionary.Keys)
    {
        upTrend = ema18[date] > ema50[date] && ema50[date] > ema100[date] && ema100[date] > ema200[date]; 
        if (upTrendPrev != upTrend)
        {
            res.Add(date, upTrend);

            upTrendPrev = upTrend;
        } 
    }

    return new DatedSeriesNew<bool>(res);
}


var resUpBounce = BackTestBounce(seriesAsc.ToDictionary(o => o.Key, o => o), ema18, ema50, ema100, ema200);
DatedSeriesNew<bounceState> BackTestBounce(Dictionary<DateTime, KeyValuePair<DateTime, QuoteDtoYahoo>> dictionary, DatedSeriesNew<double> ema18, DatedSeriesNew<double> ema50, DatedSeriesNew<double> ema100, DatedSeriesNew<double> ema200)
{
    var state = bounceState.None;
    var res = new Dictionary<DateTime, bool>();
    var states = new Dictionary<DateTime, bounceState>();

    // Determine the earliest date to back test from 
    var dateStart = dictionary.First().Key;

    // Loop through data and find interesting dates
    bool upTrend;
    bool upTrendPrev = false;
    
    foreach (DateTime date in dictionary.Keys)
    {
        upTrend = ema18[date] > ema50[date] && ema50[date] > ema100[date] && ema100[date] > ema200[date]; 
        if (!upTrend && states.Last().Value != bounceState.None)
        {
            state = bounceState.None;
            states.Add(date, state);
        }
        if (upTrendPrev != upTrend)
        {
            if (upTrend)
            {
                state = bounceState.UpTrend;
                states.Add(date, state);
            }
            res.Add(date, upTrend);

            upTrendPrev = upTrend;
        } else if (
            states.Last().Value == bounceState.UpTrend && 
            state != bounceState.Reaction && 
            dictionary[date].Value.Close < ema18[date])
        {
            state = bounceState.Reaction;
            states.Add(date, state);
        } else if (
            states.Last().Value == bounceState.Reaction && 
            state != bounceState.Buy && 
            dictionary[date].Value.Close > ema18[date] && upTrend)
        {
            state = bounceState.Buy;
            states.Add(date, state);

            // Create holding
            /*
            var holding = new Holding();
            holding.
            */

            // Create stop loss

            // Place buy

            // Update Account

        } 
    }

    return new DatedSeriesNew<bounceState>(states);
}


Console.ReadKey();



namespace Domain
{
    enum bounceState
    {
        None = 0,
        UpTrend = 1,
        Reaction = 2,
        Buy = 3 
    }

    public abstract class StrategyState { }
    
    public abstract class BounceState : StrategyState
    {
        public readonly static BounceState None = new N();
        public readonly static BounceState UpTrend = new U();
        public readonly static BounceState Reaction = new R();
        public readonly static BounceState Buy = new B();
            
        private BounceState() { }

        private class N : BounceState { }

        private class U : BounceState { }

        private class R : BounceState { }

        private class B : BounceState { } 

    }

    public class Accout
    {
        private double balance;
        public void Withdraw(double amount)
        {
            if (balance >= amount)
            { 
                balance -= amount;
            } 
        }

        public void Deposit(double amount)
        { 
            balance += amount;
        }
    }

    public class Holding
    {
        public string Ticker { get; set; }
        public double BuyingPrice { get; set; }
        public double Courtage { get; set; } 
    }


    public static class EmaFanSeriesExtentions
    {
        public static EmaFanSeries ToEmaFanSeries(this IEnumerable<EmaFanEntry> source)
        {
            return new EmaFanSeries(source.ToDictionary(o => o.DateTime, o => o));
        }

        public static IEnumerable<DatedSeries<bool>> UpTrend(this EmaFanSeries series)
        {
            return series.Select(o => new DatedSeries<bool> {
                DateTime = o.Key, 
                Value = o.Value.Value18 > o.Value.Value50 && o.Value.Value50 > o.Value.Value100 && o.Value.Value100 > o.Value.Value200 
            });
        }

    }

    public class EmaFanSeries : IDictionary<DateTime, EmaFanEntry>
    {
        private IDictionary<DateTime, EmaFanEntry> Series { get; }
        public EmaFanSeries(IDictionary<DateTime, EmaFanEntry> series)
        {
            Series = series ?? throw new ArgumentNullException(nameof(series));
        }

        public EmaFanEntry this[DateTime key] { get => Series[key]; set => Series[key] = value; }

        public ICollection<DateTime> Keys => Series.Keys;

        public ICollection<EmaFanEntry> Values => Series.Values;

        public int Count => Series.Count;

        public bool IsReadOnly => Series.IsReadOnly;


        public void Add(DateTime key, EmaFanEntry value)
        {
            Series.Add(key, value);
        }

        public void Add(KeyValuePair<DateTime, EmaFanEntry> item)
        {
            Series.Add(item);
        }

        public void Clear()
        {
            Series?.Clear();
        }

        public bool Contains(KeyValuePair<DateTime, EmaFanEntry> item)
        {
            return Series.Contains(item);
        }

        public bool ContainsKey(DateTime key)
        {
            return ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<DateTime, EmaFanEntry>[] array, int arrayIndex)
        {
            Series.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<DateTime, EmaFanEntry>> GetEnumerator()
        {
            return Series.GetEnumerator();
        }

        public bool Remove(DateTime key)
        {
            return Series.Remove(key);
        }

        public bool Remove(KeyValuePair<DateTime, EmaFanEntry> item)
        {
            return Series.Remove(item);
        }

        public bool TryGetValue(DateTime key, [MaybeNullWhen(false)] out EmaFanEntry value)
        {
            return Series.TryGetValue(key, out value);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class State
    {
        public DateTime Date { get; set; }
        public bool Buy { get; set; }
        public bool Sell { get; set; }

        public bool UpTrend { get; set; }
        public bool DownTrend { get; set; }

        public bool Reaction { get; set; }

        public bool Resume { get; set; }


        public void SetTrend()
        {

        }

    }
 
    public class DatedSeriesNew<T> : IDictionary<DateTime, T> 
    {
        public IDictionary<DateTime, T> Series { get; }

        public ICollection<DateTime> Keys => Series.Keys;

        public ICollection<T> Values => Series.Values;

        public int Count => Series.Count;

        public bool IsReadOnly => Series.IsReadOnly;

        public T this[DateTime key] { get => Series[key]; set => this[key] = value; }

        public DatedSeriesNew(IDictionary<DateTime, T> series)
        {
            Series = series ?? throw new ArgumentNullException(nameof(series));
        }

        public void Add(DateTime key, T value)
        {
            Series.Add(key, value);
        }

        public bool ContainsKey(DateTime key)
        {
            return Series.ContainsKey(key);
        }

        public bool Remove(DateTime key)
        {
            return Series.Remove(key);
        }

        public bool TryGetValue(DateTime key, [MaybeNullWhen(false)] out T value)
        {
            return Series.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<DateTime, T> item)
        {
            Series.Add(item);
        }

        public void Clear()
        {
            Series?.Clear();
        }

        public bool Contains(KeyValuePair<DateTime, T> item)
        {
            return Series.Contains(item);
        }

        public void CopyTo(KeyValuePair<DateTime, T>[] array, int arrayIndex)
        {
            Series.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<DateTime, T> item)
        {
            return Series.Remove(item);
        }

        public IEnumerator<KeyValuePair<DateTime, T>> GetEnumerator()
        {
            return Series.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Series.GetEnumerator();
        }
    }

    public class DatedSeries<T>
    {
        public DateTime DateTime { get; set; }
        public T? Value { get; set; }
    }

    public class EmaFanEntry
    {
        public DateTime DateTime { get; set; }
        public double Value18 { get; set; }
        public double Value50 { get; set; }
        public double Value100 { get; set; }
        public double Value200 { get; set; }

    }

    public class PriceAction
    {

    }
}

namespace Domain.Market
{
    public class Market
    {
        public IEnumerable<Holding> Holdings { get; set; }
        public Accout Account { get; set; }

        public Market()
        { 
        }

        public void Buy(Holding holding)
        {

        }

        public void Sell(Holding holding)
        {

        }

    }
}

namespace Dto
{
    public readonly record struct DatedValue(DateTime Date, double Value);
    public readonly record struct QuoteDto(DateTime Date, double Open, double High, double Low, double Close, int Volume, int OpenInt);
    public readonly record struct QuoteDtoYahoo(DateTime Date, double Open, double High, double Low, double Close, double AdjClose, long Volume);
    public class Quote
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double AdjClose { get; set; }
        public int Volume { get; set; }
    }
}


