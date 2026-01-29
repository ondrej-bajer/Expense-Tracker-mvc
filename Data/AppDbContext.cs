using Microsoft.EntityFrameworkCore;
using Expense_Tracker_mvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Expense_Tracker_mvc.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionCategory> TransactionCategories { get; set; }
    }
}
