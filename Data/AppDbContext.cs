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

        public DbSet<FxRate> FxRates { get; set; }
        public DbSet<FxRateFetchLog> FxRateFetchLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FxRate>()
                .HasIndex(x => new { x.Date, x.Code })
                .IsUnique();

            modelBuilder.Entity<FxRate>()
                .Property(x => x.Code)
                .HasMaxLength(3)
                .IsRequired();

            modelBuilder.Entity<FxRateFetchLog>()
                .HasIndex(x => x.RequestedDate)
                .IsUnique();

            modelBuilder.Entity<FxRateFetchLog>()
                .Property(x => x.Status)
                .HasMaxLength(16)
                .IsRequired();
        }
    }
}
