namespace Finance.Api.Domain
{
    public class EmaFanEntry
    {
        public DateTime DateTime { get; set; }
        public double Value18 { get; set; }
        public double Value50 { get; set; }
        public double Value100 { get; set; }
        public double Value200 { get; set; }

    }
}
