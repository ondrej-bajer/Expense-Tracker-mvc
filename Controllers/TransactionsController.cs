using Expense_Tracker_mvc.Data;
using Expense_Tracker_mvc.Models;
using Expense_Tracker_mvc.Models.Enums;
using Expense_Tracker_mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Expense_Tracker_mvc.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(TransactionsIndexVm vm)
        {
            // Base query
            var query = _context.Transactions
                .Include(t => t.Category)
                .AsQueryable();

            // Filters
            if (vm.Type.HasValue)
                query = query.Where(t => t.Type == vm.Type.Value);

            if (vm.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == vm.CategoryId.Value);

            if (vm.From.HasValue)
                query = query.Where(t => t.Date >= vm.From.Value);

            if (vm.To.HasValue)
                query = query.Where(t => t.Date <= vm.To.Value);

            if (vm.AmountMin.HasValue)
                query = query.Where(t => t.Amount >= vm.AmountMin.Value);

            if (vm.AmountMax.HasValue)
                query = query.Where(t => t.Amount <= vm.AmountMax.Value);

            if (!string.IsNullOrWhiteSpace(vm.Search))
            {
                var s = vm.Search.Trim();
                query = query.Where(t =>
                    (t.Description != null && t.Description.Contains(s)) ||
                    (t.Category != null && t.Category.Name.Contains(s)));
            }

            // Ordering based first on date then creation date
            query = query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.CreatedAt);

            // Data
            vm.Items = await query.ToListAsync();

            // Summary (based on filtered items)
            vm.TotalIncome = vm.Items
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            vm.TotalExpense = vm.Items
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            // Dropdowns
            vm.Categories = await _context.TransactionCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = vm.CategoryId.HasValue && c.Id == vm.CategoryId.Value
                })
                .ToListAsync();

            vm.Types = Enum.GetValues(typeof(TransactionType))
                .Cast<TransactionType>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = t.ToString(),
                    Selected = vm.Type.HasValue && t == vm.Type.Value
                })
                .ToList();

            return View(vm);
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(
                _context.TransactionCategories.Where(c => c.IsActive),
                "Id",
                "Name"
            );

            var model = new Transaction
            {
                Date = DateTime.Today
            };

            return View(model);
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Amount,Type,CategoryId,Description")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.CreatedAt = DateTime.Now;

                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // In a case of wrong validation fill dropdown + keep chosen value
            ViewData["CategoryId"] = new SelectList(
                _context.TransactionCategories.Where(c => c.IsActive),
                "Id",
                "Name",
                transaction.CategoryId
            );

            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(
                _context.TransactionCategories.Where(c => c.IsActive),
                "Id",
                "Name",
                transaction.CategoryId
            );

            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Amount,Type,CategoryId,Description")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // keep CreatedAt but dont edit it in form
                    var existing = await _context.Transactions.AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (existing == null)
                    {
                        return NotFound();
                    }

                    transaction.CreatedAt = existing.CreatedAt;

                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // In a case of wrong validation "fallback" for dropdown
            ViewData["CategoryId"] = new SelectList(
                _context.TransactionCategories.Where(c => c.IsActive),
                "Id",
                "Name",
                transaction.CategoryId
            );

            return View(transaction);
        }

        // GET: Transactions/Delete/-
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
