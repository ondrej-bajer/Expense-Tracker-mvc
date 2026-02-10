using Expense_Tracker_mvc.Data;
using Expense_Tracker_mvc.Models;
using Expense_Tracker_mvc.Models.Enums;
using Expense_Tracker_mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;


namespace Expense_Tracker_mvc.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public HomeController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var today = DateTime.Today;
            var from = new DateTime(today.Year, today.Month, 1);
            var to = from.AddMonths(1); // exclusive

            var monthItems = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.OwnerId == userId)
                .Where(t => t.Date >= from && t.Date < to)
                .ToListAsync();

            // ===== 12 months range (including current month) =====
            var seriesStart = from.AddMonths(-11);
            var seriesEndExclusive = to;

            // Load transactions for last 12 months
            var tx12m = await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.OwnerId == userId)
                .Where(t => t.Date >= seriesStart && t.Date < seriesEndExclusive)
                .ToListAsync();

            // ===== Chart 1: Income vs Expense (12 months) =====
            var incomeExpenseSeries = BuildMonthlySeries(tx12m, seriesStart, 12);

            // ===== Chart 2: Expense by category (this month) =====
            var expenseByCategoryThisMonth = monthItems
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category?.Name ?? "-")
                .Select(g => new CategorySliceVm
                {
                    Category = g.Key,
                    Amount = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Income/Expense in-memory (SQLite problem with SUM(decimal))
            var income = monthItems
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var expense = monthItems
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            // Top category this month
            var topCategory = monthItems
                .Where(t => t.Category != null)
                .GroupBy(t => t.Category!.Name)
                .Select(g => new { Category = g.Key, Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            var last5 = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.OwnerId == userId)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vm = new HomeDashboardVm
            {
                TotalIncomeThisMonth = income,
                TotalExpenseThisMonth = expense,
                TopCategoryThisMonth = topCategory?.Category ?? "-",
                Last5Transactions = last5,

                IncomeExpenseLast12Months = incomeExpenseSeries,
                ExpenseByCategoryThisMonth = expenseByCategoryThisMonth
            };

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static List<MonthlySeriesPointVm> BuildMonthlySeries(
    List<Transaction> tx,
    DateTime startMonth,
    int months)
        {
            var dict = tx
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .ToDictionary(
                    g => (g.Key.Year, g.Key.Month),
                    g => new
                    {
                        Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount),
                        Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
                    });

            var result = new List<MonthlySeriesPointVm>(months);

            for (int i = 0; i < months; i++)
            {
                var m = startMonth.AddMonths(i);
                dict.TryGetValue((m.Year, m.Month), out var agg);

                result.Add(new MonthlySeriesPointVm
                {
                    Month = m.ToString("yyyy-MM"),
                    Income = agg?.Income ?? 0m,
                    Expense = agg?.Expense ?? 0m
                });
            }

            return result;
        }
    }
}
