namespace Expense_Tracker_mvc.Models.Import
{
    public sealed record ImportResult(
        bool Success,
        int ImportedCount,
        int CreatedCategoriesCount,
        List<string> Errors)
    {
        public static ImportResult Ok(int imported, int createdCategories)
            => new(true, imported, createdCategories, new());

        public static ImportResult Fail(List<string> errors)
            => new(false, 0, 0, errors);

        public static ImportResult Fail(params string[] errors)
            => new(false, 0, 0, errors.ToList());
    }
}
