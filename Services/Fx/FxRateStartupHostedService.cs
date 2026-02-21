using Microsoft.Extensions.Hosting;

namespace Expense_Tracker_mvc.Services.Fx
{
    public class FxRateStartupHostedService : IHostedService
    {
        private readonly IServiceProvider _sp;

        public FxRateStartupHostedService(IServiceProvider sp)
        {
            _sp = sp;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _sp.CreateScope();
            var fx = scope.ServiceProvider.GetRequiredService<IFxRateService>();
            await fx.EnsureRatesForTodayAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
