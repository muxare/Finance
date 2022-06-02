// See https://aka.ms/new-console-template for more information
using Dto;
using Finance.Api.Domain;
using Finance.Api.Models;
using System.Globalization;

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

DatedSeries<double> EmaInvestiopedia(DatedSeries<double> list, int days)
{
    var smoothing = 2.0;
    var previous = 0.0;
    var beta = smoothing / (1 + days);
    var series = list.Select((d, i) =>
    {
        var current = d.Value * beta + previous * (1.0 - beta);
        previous = current;
        return new { DateTime = d.Key, Value = current };
    }).ToDictionary(o => o.DateTime, o => o.Value);

    var ema = new DatedSeries<double>(series);

    return ema;
}


/*
Domain.DatedSeriesNew<PriceAction> PriceActionSeries()
{

}
*/



var seriesAsc = series.OrderBy(q => q.Key);
var seriesDesc = series.OrderByDescending(q => q.Key);

var closeSeries = new DatedSeries<double>(seriesAsc.ToDictionary(o => o.Key, o => o.Value.Close.Value));
var ema18 = EmaInvestiopedia(closeSeries, 18);
var ema50 = EmaInvestiopedia(closeSeries, 50);
var ema100 = EmaInvestiopedia(closeSeries, 100);
var ema200 = EmaInvestiopedia(closeSeries, 200);

EmaFanSeries list = closeSeries.Select(q => new EmaFanEntry
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

DatedSeries<bool> BackTestUpTrend(Dictionary<DateTime, KeyValuePair<DateTime, QuoteDtoYahoo>> dictionary, DatedSeries<double> ema18, DatedSeries<double> ema50, DatedSeries<double> ema100, DatedSeries<double> ema200)
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

    return new DatedSeries<bool>(res);
}


var resUpBounce = BackTestBounce(seriesAsc.ToDictionary(o => o.Key, o => o), ema18, ema50, ema100, ema200);
DatedSeries<bounceState> BackTestBounce(Dictionary<DateTime, KeyValuePair<DateTime, QuoteDtoYahoo>> dictionary, DatedSeries<double> ema18, DatedSeries<double> ema50, DatedSeries<double> ema100, DatedSeries<double> ema200)
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

    return new DatedSeries<bounceState>(states);
}


Console.ReadKey();




namespace Dto
{
    
}


