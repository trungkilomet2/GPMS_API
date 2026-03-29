using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class DailyJobService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DailyJobService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            // chạy lúc 00:00
            var nextRun = now.Date.AddDays(1);

            var delay = nextRun - now;

            await Task.Delay(delay, stoppingToken);

            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<GPMS_SYSTEMContext>();

            await db.Database.ExecuteSqlRawAsync(@"
                UPDATE [PART_WORK_LOG]
                SET IS_READ_ONLY = 1
                WHERE CAST(CREATE_DATE AS DATE) < CAST(GETDATE() AS DATE)
                AND IS_READ_ONLY = 0
            ");
        }
    }
}