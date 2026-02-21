namespace Expense_Tracker_mvc.Models
{
    public class FxRateFetchLog
    {
        public int Id { get; set; }

        // „Dnes“ (podle Europe/Prague), date-only 00:00
        public DateTime RequestedDate { get; set; }

        // PublishedDate z hlavičky ČNB, může být starší (víkend/svátek)
        public DateTime PublishedDate { get; set; }

        public DateTime LastAttemptUtc { get; set; }
        public DateTime? LastSuccessUtc { get; set; }

        // Never / Success / Failed
        public string Status { get; set; } = "Never";

        public string? Error { get; set; }

    }
}
