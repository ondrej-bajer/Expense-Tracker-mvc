using Expense_Tracker_mvc.Data;
using Expense_Tracker_mvc.Models;
using Expense_Tracker_mvc.Models.Enums;
using Expense_Tracker_mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Identity;


namespace Expense_Tracker_mvc.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(TransactionsIndexVm vm)
        {
            //for selecting user
            var userId = _userManager.GetUserId(User);
            // Base query
            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.OwnerId == userId)
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
                .Where(c => c.IsActive && c.OwnerId == userId) //select dropdown based on user
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

            var userId = _userManager.GetUserId(User);

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);

            ViewData["CategoryId"] = new SelectList(
                _context.TransactionCategories.Where(c => c.IsActive && c.OwnerId == userId),
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
            var userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                // Ownership + timestamps server-side
                transaction.OwnerId = userId;
                transaction.CreatedAt = DateTime.Now;

                //Ensure selected category belongs to current user
                var categoryOk = await _context.TransactionCategories
                    .AnyAsync(c => c.Id == transaction.CategoryId && c.IsActive && c.OwnerId == userId);

                if (!categoryOk)
                {
                    return NotFound();
                }

                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // In a case of wrong validation fill dropdown + keep chosen value
            ViewData["CategoryId"] = new SelectList(
                _context.TransactionCategories
                    .Where(c => c.IsActive && c.OwnerId == userId),
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

            var userId = _userManager.GetUserId(User);

            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(
                _context.TransactionCategories.Where(c => c.IsActive && c.OwnerId == userId),
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

            var userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                // load only current user's transaction
                var existing = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == userId);

                if (existing == null)
                {
                    return NotFound(); // neprozrazuje, že cizí existuje
                }

                // validate category belongs to user
                var categoryOk = await _context.TransactionCategories
                    .AnyAsync(c => c.Id == transaction.CategoryId && c.IsActive && c.OwnerId == userId);

                if (!categoryOk)
                {
                    return NotFound();
                }

                // update allowed fields only
                existing.Date = transaction.Date;
                existing.Amount = transaction.Amount;
                existing.Type = transaction.Type;
                existing.CategoryId = transaction.CategoryId;
                existing.Description = transaction.Description;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // check existence WITH ownership
                    var stillExists = await _context.Transactions
                        .AnyAsync(t => t.Id == id && t.OwnerId == userId);

                    if (!stillExists)
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // dropdown only current user's categories
            ViewData["CategoryId"] = new SelectList(
                _context.TransactionCategories.Where(c => c.IsActive && c.OwnerId == userId),
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

            var userId = _userManager.GetUserId(User);

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);
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
            var userId = _userManager.GetUserId(User);

            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            var userId = _userManager.GetUserId(User);

            return _context.Transactions
                .Any(e => e.Id == id && e.OwnerId == userId);
        }
    }
}
