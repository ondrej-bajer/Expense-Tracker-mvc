using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_mvc.Models.Enums
{
    public enum TransactionType
    {
        [Display(Name = "Income")]
        Income = 1,

        [Display(Name = "Expense")]
        Expense = 2
    }
}
