using MassTransit;

namespace Sieg.ConsumerWorker;

public class Worker : BackgroundService
{
    private readonly IBusControl _bus;
    private readonly ILogger<Worker> _logger;

    public Worker(IBusControl bus, ILogger<Worker> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
