using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker_mvc.Data;

namespace Expense_Tracker_mvc.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly AppDbContext _db;

        public SettingsController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> FxRates()
        {
            var today = DateTime.Today;

            // zkus dnešek
            var rates = await _db.FxRates
                .Where(x => x.Date == today)
                .OrderBy(x => x.Code)
                .ToListAsync();

            // fallback: poslední dostupný den (víkend/svátek)
            if (rates.Count == 0)
            {
                var lastDate = await _db.FxRates.MaxAsync(x => (DateTime?)x.Date);
                if (lastDate is not null)
                {
                    rates = await _db.FxRates
                        .Where(x => x.Date == lastDate.Value)
                        .OrderBy(x => x.Code)
                        .ToListAsync();

                    ViewBag.RatesDate = lastDate.Value;
                }
            }
            else
            {
                ViewBag.RatesDate = today;
            }

            return View(rates);
        }
    }
}
