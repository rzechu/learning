using Mongo.WebApi.Services;

namespace Mongo.WebApi.BackgroundServices;

public class OrderPaymentScheduler : BackgroundService
{
    private readonly ILogger<OrderPaymentScheduler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderPaymentScheduler(ILogger<OrderPaymentScheduler> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Payment Scheduler running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Order Payment Scheduler checking for orders to pay at: {time}", DateTimeOffset.Now);

            using (var scope = _scopeFactory.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

                var ordersToPay = (await orderService.GetAsync())
                                    .Where(o => o.Status == "Pending" && (DateTime.UtcNow - o.CreatedAt).TotalSeconds >= 15)
                                    .ToList();

                foreach (var order in ordersToPay)
                {
                    _logger.LogInformation("Attempting to set Order ID {OrderId} to Paid.", order.Id);
                    await orderService.SetOrderPaidAsync(order.Id);
                    _logger.LogInformation("Order ID {OrderId} status set to Paid.", order.Id);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); 
        }

        _logger.LogInformation("Order Payment Scheduler stopped.");
    }
}