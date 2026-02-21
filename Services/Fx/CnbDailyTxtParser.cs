using Expense_Tracker_mvc.Models;
using System.Globalization;

namespace Expense_Tracker_mvc.Services.Fx
{
    public static class CnbDailyTxtParser
    {
        public static (DateTime publishedDate, List<FxRate> rates) Parse(string text)
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 0)
                            .ToArray();

            if (lines.Length < 3)
                throw new InvalidOperationException("Unexpected CNB daily.txt format.");

            // "20 Feb 2026 #36"
            var header = lines[0];
            var datePart = header.Split('#')[0].Trim(); // "20 Feb 2026"
            var dt = DateTime.ParseExact(datePart, "d MMM yyyy", CultureInfo.InvariantCulture);
            var publishedDate = dt.Date;

            var inv = CultureInfo.InvariantCulture;

            var result = new List<FxRate>();

            for (int i = 2; i < lines.Length; i++)
            {
                var parts = lines[i].Split('|');
                if (parts.Length != 5) continue;

                var country = parts[0].Trim();
                var currencyName = parts[1].Trim();
                var amount = int.Parse(parts[2].Trim(), inv);
                var code = parts[3].Trim();
                var rate = decimal.Parse(parts[4].Trim(), inv);

                result.Add(new FxRate
                {
                    Date = publishedDate,
                    Country = country,
                    CurrencyName = currencyName,
                    Amount = amount,
                    Code = code,
                    Rate = rate,
                    RatePerUnit = rate / amount,
                    DownloadedAtUtc = DateTime.UtcNow
                });
            }

            // doplň CZK pro pohodlí
            result.Add(new FxRate
            {
                Date = publishedDate,
                Code = "CZK",
                Amount = 1,
                Rate = 1m,
                RatePerUnit = 1m,
                Country = "Czech Republic",
                CurrencyName = "koruna",
                DownloadedAtUtc = DateTime.UtcNow
            });

            return (publishedDate, result);
        }
    }
}
