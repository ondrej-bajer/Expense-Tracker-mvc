using Expense_Tracker_mvc.Models;

namespace Expense_Tracker_mvc.ViewModels
{
    public class HomeDashboardVm
    {
        public decimal TotalIncomeThisMonth { get; set; }
        public decimal TotalExpenseThisMonth { get; set; }

        public decimal BalanceThisMonth =>
            TotalIncomeThisMonth - TotalExpenseThisMonth;

        public string TopCategoryThisMonth { get; set; } = "-";

        public List<Transaction> Last5Transactions { get; set; } = new();

        public List<MonthlySeriesPointVm> IncomeExpenseLast12Months { get; set; } = new();
        public List<CategorySliceVm> ExpenseByCategoryThisMonth { get; set; } = new();
    }


}
