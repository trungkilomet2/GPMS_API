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

    // Tự động xử lý trong cở sở dữ liệu để set IS_READ_ONLY = 1
    // cho các bản ghi cũ hơn ngày hiện tại trong
    // PART_WORK_LOG và CUTTING_NOTEBOOK_LOG,
    // chạy vào lúc 00:00 hàng ngày
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
            await db.Database.ExecuteSqlRawAsync(@"
                UPDATE [CUTTING_NOTEBOOK_LOG]
                SET IS_READ_ONLY = 1
                WHERE CAST(DATE_CREATE AS DATE) < CAST(GETDATE() AS DATE)
                AND IS_READ_ONLY = 0
            ");
        }
    }
}