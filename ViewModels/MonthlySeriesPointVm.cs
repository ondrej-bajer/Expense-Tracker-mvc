namespace Expense_Tracker_mvc.ViewModels
{
    public class MonthlySeriesPointVm
    {
        public string Month { get; set; } = ""; // "2026-02" x "Feb 2026"
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }
}
