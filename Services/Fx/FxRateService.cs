using Expense_Tracker_mvc.Data;
using Expense_Tracker_mvc.Models;
using Microsoft.EntityFrameworkCore;


namespace Expense_Tracker_mvc.Services.Fx
{
    public class FxRateService : IFxRateService
    {
        private readonly AppDbContext _db;
        private readonly HttpClient _http;

        // cooldown po failu (aby to nespamovalo)
        private static readonly TimeSpan FailCooldown = TimeSpan.FromMinutes(30);

        public FxRateService(AppDbContext db, HttpClient http)
        {
            _db = db;
            _http = http;
        }

        public async Task EnsureRatesForTodayAsync(CancellationToken ct = default)
        {
            // CZ lokální “dnes”
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague");
            var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;

            await EnsureRatesForRequestedDateAsync(todayLocal, ct);
        }

        private async Task EnsureRatesForRequestedDateAsync(DateTime requestedDate, CancellationToken ct)
        {
            requestedDate = requestedDate.Date;
            var nowUtc = DateTime.UtcNow;

            var log = await _db.FxRateFetchLogs.SingleOrDefaultAsync(x => x.RequestedDate == requestedDate, ct);

            // už dnes úspěšně staženo => hotovo
            if (log?.Status == "Success" && log.LastSuccessUtc.HasValue)
                return;

            // fail throttle
            if (log?.Status == "Failed" && (nowUtc - log.LastAttemptUtc) < FailCooldown)
                return;

            if (log is null)
            {
                log = new FxRateFetchLog
                {
                    RequestedDate = requestedDate,
                    LastAttemptUtc = nowUtc,
                    Status = "Never"
                };
                _db.FxRateFetchLogs.Add(log);
                await _db.SaveChangesAsync(ct);
            }

            log.LastAttemptUtc = nowUtc;

            try
            {
                var url = BuildCnbUrl(requestedDate);
                var text = await _http.GetStringAsync(url, ct);

                var (publishedDate, rates) = CnbDailyTxtParser.Parse(text);

                // upsert FxRates by (publishedDate, code)
                foreach (var r in rates)
                {
                    r.Date = publishedDate.Date;
                    r.DownloadedAtUtc = nowUtc;

                    var existing = await _db.FxRates
                        .FirstOrDefaultAsync(x => x.Date == publishedDate.Date && x.Code == r.Code, ct);

                    if (existing is null)
                    {
                        _db.FxRates.Add(r);
                    }
                    else
                    {
                        existing.Amount = r.Amount;
                        existing.Rate = r.Rate;
                        existing.RatePerUnit = r.RatePerUnit;
                        existing.Country = r.Country;
                        existing.CurrencyName = r.CurrencyName;
                        existing.DownloadedAtUtc = nowUtc;
                    }
                }

                // důležité: logujeme requestedDate (dnes) + publishedDate (co ČNB vrátila)
                log.PublishedDate = publishedDate.Date;
                log.Status = "Success";
                log.LastSuccessUtc = nowUtc;
                log.Error = null;

                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                log.Status = "Failed";
                log.Error = ex.Message;
                await _db.SaveChangesAsync(ct);

                // při startu aplikace klidně neházej dál, ať appka nastartuje
                // když chceš vidět chybu v logu, můžeš to rethrow-nout
                // throw;
            }
        }

        private static string BuildCnbUrl(DateTime date)
        {
            // Parametr date je dd.MM.yyyy (ČNB)
            return "https://www.cnb.cz/en/financial_markets/foreign_exchange_market/" +
                   "exchange_rate_fixing/daily.txt?date=" + date.ToString("dd.MM.yyyy");
        }
    }
}
