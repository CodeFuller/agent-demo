using System;
using System.Threading;
using System.Threading.Tasks;
using CodeFuller.Library.Bootstrap;
using DemoAgent.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DemoAgent
{
	internal class ApplicationLogic : IApplicationLogic
	{
		private readonly IMetricsCollector metricsCollector;

		private readonly ILogger<ApplicationLogic> logger;

		private readonly AgentSettings settings;

		public ApplicationLogic(IMetricsCollector metricsCollector, ILogger<ApplicationLogic> logger, IOptions<AgentSettings> options)
		{
			this.metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public async Task<int> Run(string[] args, CancellationToken cancellationToken)
		{
			try
			{
				await RunInternal(cancellationToken);

				return 0;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				logger.LogCritical(e, "Application has failed");
				return e.HResult;
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
