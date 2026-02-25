using CsvHelper;
using Expense_Tracker_mvc.Data;
using Expense_Tracker_mvc.Models;
using Expense_Tracker_mvc.Models.Import;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CsvHelperReader = CsvHelper.CsvReader;





namespace Expense_Tracker_mvc.Services.Import
{
    public sealed class TransactionImportService
    {
        private readonly AppDbContext _db;

        public TransactionImportService(AppDbContext db) => _db = db;

        public async Task<ImportResult> ImportCsvAsync(Stream csvStream, string ownerId, CancellationToken ct)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvHelperReader(reader, CultureInfo.InvariantCulture);

            List<TransactionImportRow> rows;

            try
            {
                rows = csv.GetRecords<TransactionImportRow>().ToList();
            }
            catch (CsvHelperException ex)
            {
                return ImportResult.Fail(
                    $"CSV parse error on row {ex.Context.Parser.Row}: {ex.Message}"
                );
            }
            catch (Exception ex)
            {
                return ImportResult.Fail($"CSV parse error: {ex.Message}");
            }

            if (rows.Count == 0)
                return ImportResult.Fail("CSV contains no rows.");

            // 1) Validace DTO
            var errors = new List<string>();
            for (int i = 0; i < rows.Count; i++)
            {
                var ctx = new ValidationContext(rows[i]);
                var results = new List<ValidationResult>();

                if (!Validator.TryValidateObject(rows[i], ctx, results, validateAllProperties: true))
                {
                    foreach (var r in results)
                        errors.Add($"Row {i + 2}: {r.ErrorMessage}");
                }
            }

            if (errors.Count > 0)
                return ImportResult.Fail(errors);

            // 2) Načti existující kategorie pro uživatele
            var existingCategories = await _db.TransactionCategories
                .Where(c => c.OwnerId == ownerId)
                .ToListAsync(ct);

            var categoryDict = existingCategories.ToDictionary(
                c => c.Name.Trim(),
                c => c,
                StringComparer.OrdinalIgnoreCase);

            var newCategories = new List<TransactionCategory>();
            var transactions = new List<Transaction>(rows.Count);

            foreach (var row in rows)
            {
                var categoryName = row.CategoryName.Trim();

                if (!categoryDict.TryGetValue(categoryName, out var category))
                {
                    category = new TransactionCategory
                    {
                        OwnerId = ownerId,
                        Name = categoryName,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    newCategories.Add(category);
                    categoryDict[categoryName] = category;
                }

                transactions.Add(new Transaction
                {
                    OwnerId = ownerId,
                    Date = row.Date,
                    Amount = row.Amount,
                    Type = row.Type,
                    Category = category,
                    Description = row.Description?.Trim(),
                    CreatedAt = DateTime.Now
                });
            }

            var createdCategories = newCategories.Count;

            // 3) Ulož vše v jedné transakci
            await using var trx = await _db.Database.BeginTransactionAsync(ct);

            if (newCategories.Count > 0)
                _db.TransactionCategories.AddRange(newCategories);

            _db.Transactions.AddRange(transactions);

            await _db.SaveChangesAsync(ct);
            await trx.CommitAsync(ct);

            return ImportResult.Ok(transactions.Count, createdCategories);
        }
    }
}
