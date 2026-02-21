namespace Expense_Tracker_mvc.Models
{
    public class FxRate
    {
        public int Id { get; set; }

        // Published date ČNB (date-only 00:00)
        public DateTime Date { get; set; }

        // ISO 4217: EUR, USD...
        public string Code { get; set; } = default!;

        // z ČNB: amount (1, 100, ...)
        public int Amount { get; set; }

        // z ČNB: CZK za Amount
        public decimal Rate { get; set; }

        // CZK za 1 jednotku
        public decimal RatePerUnit { get; set; }

        public string? Country { get; set; }
        public string? CurrencyName { get; set; }

        public DateTime DownloadedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
