using System;
using System.Threading;
using System.Threading.Tasks;
using BluetoothConnectTest;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConnectTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var watcher = new DnaBluetoothLEAdvertisementWatcher();
                watcher.StartedListening += () =>
                {
                    _logger.LogInformation("Started Listening");
                };
                watcher.StoppedListening += () =>
                {
                    _logger.LogInformation("Stopped Listening");
                };
                watcher.NewDeviceDiscovered += (device) =>
                {
                    _logger.LogInformation($"New device: {device}");
                };
                watcher.StartListening();

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
} 
