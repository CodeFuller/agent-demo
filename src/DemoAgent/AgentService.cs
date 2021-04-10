using System;
using System.Threading;
using System.Threading.Tasks;
using DemoAgent.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DemoAgent
{
	internal class AgentService : BackgroundService
	{
		private readonly IMetricsCollector metricsCollector;

		private readonly ILogger<AgentService> logger;

		private readonly AgentSettings settings;

		public AgentService(IMetricsCollector metricsCollector, ILogger<AgentService> logger, IOptions<AgentSettings> options)
		{
			this.metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Starting the service ...");

			return base.StartAsync(cancellationToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Stopping the service ...");

			return base.StopAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			logger.LogInformation("Running the service ...");

			try
			{
				await RunInternal(stoppingToken);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				// TODO: The service should terminate if we get here.
				logger.LogCritical(e, "Application has failed");
				throw;
			}
		}

		private async Task RunInternal(CancellationToken cancellationToken)
		{
			var connection = new HubConnectionBuilder()
				.WithUrl(settings.ServerAddress)
				.WithAutomaticReconnect()
				.Build();

			connection.Reconnecting += e =>
			{
				logger.LogWarning(e, "Restoring connection ...");
				return Task.CompletedTask;
			};

			connection.Reconnected += _ =>
			{
				logger.LogInformation("Connection was restored successfully");
				return Task.CompletedTask;
			};

			connection.Closed += e =>
			{
				logger.LogError(e, "Connection was closed");
				return Task.CompletedTask;
			};

			logger.LogInformation("Starting connection ...");
			await connection.StartAsync(cancellationToken);
			logger.LogInformation("Connection was started successfully");

			await metricsCollector.Initialize(cancellationToken);

			while (!cancellationToken.IsCancellationRequested)
			{
				var metrics = await metricsCollector.CollectMetrics(cancellationToken);

				logger.LogInformation("Dumping metrics ...");
				await connection.InvokeAsync("DumpMetrics", metrics, cancellationToken);

				await Task.Delay(settings.MetricsUpdateInterval, cancellationToken);
			}
		}
	}
}
