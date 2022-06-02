namespace Finance.Api.Domain
{
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
}
