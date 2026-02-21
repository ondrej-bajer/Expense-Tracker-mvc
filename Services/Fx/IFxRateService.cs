namespace Expense_Tracker_mvc.Services.Fx
{
    public interface IFxRateService
    {
        Task EnsureRatesForTodayAsync(CancellationToken ct = default);
    }

}
