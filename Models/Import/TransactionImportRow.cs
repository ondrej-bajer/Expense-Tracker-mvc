using Expense_Tracker_mvc.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_mvc.Models.Import
{
    public class TransactionImportRow
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(0.1, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
