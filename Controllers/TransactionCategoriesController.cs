using Expense_Tracker_mvc.Data;
using Expense_Tracker_mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Expense_Tracker_mvc.Controllers
{
    [Authorize]
    public class TransactionCategoriesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionCategoriesController(AppDbContext context, UserManager<ApplicationUser>userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TransactionCategories
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            return View(await _context.TransactionCategories
                .Where(c => c.OwnerId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync());
        }

        // GET: TransactionCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var transactionCategory = await _context.TransactionCategories
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (transactionCategory == null)
            {
                return NotFound();
            }

            return View(transactionCategory);
        }

        // GET: TransactionCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TransactionCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,IsActive")] TransactionCategory transactionCategory)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            transactionCategory.OwnerId = userId;
            transactionCategory.CreatedAt = DateTime.UtcNow;

            if (!ModelState.IsValid)
                return View(transactionCategory);

            _context.TransactionCategories.Add(transactionCategory);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: TransactionCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var transactionCategory = await _context.TransactionCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

            if (transactionCategory == null)
            {
                return NotFound();
            }
            return View(transactionCategory);
        }

        // POST: TransactionCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive")] TransactionCategory transactionCategory)
        {
            if (id != transactionCategory.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                // load existing category only for current user
                var existing = await _context.TransactionCategories
                    .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

                if (existing == null)
                {
                    return NotFound();
                }

                // update allowed fields only
                existing.Name = transactionCategory.Name;
                existing.IsActive = transactionCategory.IsActive;
                // CreatedAt + OwnerId remain unchanged

                try
                {
                    await _context.SaveChangesAsync(); //CHANGED (no _context.Update)
                }
                catch (DbUpdateConcurrencyException)
                {
                    //existence check should include ownership
                    var stillExists = await _context.TransactionCategories
                        .AnyAsync(c => c.Id == id && c.OwnerId == userId);

                    if (!stillExists)
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(transactionCategory);
        }


        // GET: TransactionCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var transactionCategory = await _context.TransactionCategories
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (transactionCategory == null)
            {
                return NotFound();
            }

            return View(transactionCategory);
        }


        // POST: TransactionCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var transactionCategory = await _context.TransactionCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

            if (transactionCategory == null)
            {
                return NotFound();
            }

            //block delete if category is used by user's transactions
            var used = await _context.Transactions
                .AnyAsync(t => t.CategoryId == id && t.OwnerId == userId);

            if (used)
            {
                ModelState.AddModelError(string.Empty, "Category is used by existing transactions. Deactivate it instead.");
                return View("Delete", transactionCategory);
            }

            _context.TransactionCategories.Remove(transactionCategory);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TransactionCategoryExists(int id)
        {
            var userId = _userManager.GetUserId(User);

            return _context.TransactionCategories
                .Any(e => e.Id == id && e.OwnerId == userId);
        }
    }
}
