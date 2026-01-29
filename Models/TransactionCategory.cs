using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_mvc.Models
{
    public class TransactionCategory
    {
        public int Id { get; set; }

        [BindNever]
        [ValidateNever]
        public string OwnerId { get; set; } = default!;

        [BindNever]
        [ValidateNever]
        public ApplicationUser? Owner { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [BindNever]
        [ValidateNever]
        public DateTime CreatedAt {  get; set; } = DateTime.Now;
    }
}
