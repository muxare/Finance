using Microsoft.EntityFrameworkCore;

namespace Finance.Storage
{
    public class FinanceDbContext : DbContext
    {
        public DbSet<Quote> Quotes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=.;Database=Finance;Trusted_Connection=True");
    }

    public class Quote
    {
        public Guid Id { get; set; }
        public string Ticker { get; set; } = null!;
        public DateTime Date { get; set; }
        public double? Open { get; set; }
        public double? High { get; set; }
        public double? Low { get; set; }
        public double? Close { get; set; }
        public double? AdjClose { get; set; }
        public int? Volume { get; set; }
    }

    public class Ema
    {
    }
}
