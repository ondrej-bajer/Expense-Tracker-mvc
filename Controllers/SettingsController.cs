using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker_mvc.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
