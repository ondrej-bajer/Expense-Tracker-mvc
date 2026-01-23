using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_mvc.Models
{
    public class TransactionCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt {  get; set; } = DateTime.Now;
    }
}
