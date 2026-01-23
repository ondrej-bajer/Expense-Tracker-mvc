using Expense_Tracker_mvc.Models;
using Expense_Tracker_mvc.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Expense_Tracker_mvc.ViewModels

{
    public class TransactionsIndexVm
    {
        // Filters
        public TransactionType? Type { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }
        public string? Search { get; set; }

        // Dropdowns
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Types { get; set; } = new();

        // Data
        public List<Transaction> Items { get; set; } = new();

        // Summary
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;
    }
}
