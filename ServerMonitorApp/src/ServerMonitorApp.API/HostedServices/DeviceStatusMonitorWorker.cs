using MediatR;
using ServerMonitorApp.Application.Features.Alerts.Commands.CheckOfflineDevices;

namespace ServerMonitorApp.API.HostedServices
{
    public class DeviceStatusMonitorWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DeviceStatusMonitorWorker> _logger;

        public DeviceStatusMonitorWorker(IServiceScopeFactory scopeFactory, ILogger<DeviceStatusMonitorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope? scope = _scopeFactory.CreateScope();
                    IMediator? mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediator.Send(new CheckOfflineDevicesCommand(), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra khi kích hoạt CheckOfflineDevicesCommand.");
                }

                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}