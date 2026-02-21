using Expense_Tracker_mvc.Data;
using Expense_Tracker_mvc.Models;
using Expense_Tracker_mvc.Services.Fx;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Expense_Tracker_mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddHttpClient<IFxRateService, FxRateService>();
            builder.Services.AddHostedService<FxRateStartupHostedService>();

            var dbPath = Path.Combine(builder.Environment.ContentRootPath, "expense_tracker.db");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Identity for custom user
            builder.Services
                .AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<AppDbContext>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //Authentication must be before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            //Identity endpoints (/Identity/Account/Login etc.)
            app.MapRazorPages();

            app.Run();
        }
    }
}
