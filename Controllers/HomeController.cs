using Expense_Tracker_mvc.Data;
using Expense_Tracker_mvc.Models;
using Expense_Tracker_mvc.Models.Enums;
using Expense_Tracker_mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace Expense_Tracker_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;


        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var from = new DateTime(today.Year, today.Month, 1);
            var to = from.AddMonths(1); // exclusive

            var monthItems = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Date >= from && t.Date < to)
                .ToListAsync();

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
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vm = new HomeDashboardVm
            {
                TotalIncomeThisMonth = income,
                TotalExpenseThisMonth = expense,
                TopCategoryThisMonth = topCategory?.Category ?? "-",
                Last5Transactions = last5
            };

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
