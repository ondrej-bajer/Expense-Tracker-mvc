using Expense_Tracker_mvc.Data;
using Expense_Tracker_mvc.Models;
using Microsoft.AspNetCore.Authorization;
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

        public TransactionCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TransactionCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.TransactionCategories.ToListAsync());
        }

        // GET: TransactionCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionCategory = await _context.TransactionCategories
                .FirstOrDefaultAsync(m => m.Id == id);
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
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive,CreatedAt")] TransactionCategory transactionCategory)
        {
            if (ModelState.IsValid)
            {
                transactionCategory.CreatedAt = DateTime.Now;
                _context.Add(transactionCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transactionCategory);
        }

        // GET: TransactionCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionCategory = await _context.TransactionCategories.FindAsync(id);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive,CreatedAt")] TransactionCategory transactionCategory)
        {
            if (id != transactionCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionCategoryExists(transactionCategory.Id))
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
            return View(transactionCategory);
        }

        // GET: TransactionCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionCategory = await _context.TransactionCategories
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var transactionCategory = await _context.TransactionCategories.FindAsync(id);
            if (transactionCategory != null)
            {
                _context.TransactionCategories.Remove(transactionCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionCategoryExists(int id)
        {
            return _context.TransactionCategories.Any(e => e.Id == id);
        }
    }
}
