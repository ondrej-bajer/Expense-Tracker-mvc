using Expense_Tracker_mvc.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_mvc.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [BindNever, ValidateNever]
        public string OwnerId { get; set; } = default!;

        public int? CategoryId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(0.1, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [BindNever, ValidateNever]
        public TransactionCategory? Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [BindNever, ValidateNever]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
