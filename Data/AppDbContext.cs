using Microsoft.EntityFrameworkCore;
using Expense_Tracker_mvc.Models;

namespace Expense_Tracker_mvc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
    }
}
