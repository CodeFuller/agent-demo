using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using AgentDemo.Common;
using DemoAgent.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DemoAgent.Internal
{
	internal class MetricsCollector : IMetricsCollector
	{
		private readonly ILogger<MetricsCollector> logger;

		private readonly AgentSettings settings;

		private bool IsInitialized { get; set; }

		private ManagementObject TotalCpuObject { get; set; }

		private ManagementObject OperatingSystemObject { get; set; }

		public MetricsCollector(ILogger<MetricsCollector> logger, IOptions<AgentSettings> options)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public Task Initialize(CancellationToken cancellationToken)
		{
			logger.LogInformation("Initializing metrics collector ...");

			logger.LogInformation("Initializing CPU collector ...");
			using var processorSearcher = new ManagementObjectSearcher(new SelectQuery(@"SELECT Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name = '_Total'"));
			TotalCpuObject = processorSearcher.Get().OfType<ManagementObject>().SingleOrDefault();

			logger.LogInformation("Initializing memory collector ...");
			using var osSearcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory, TotalVisibleMemorySize FROM Win32_OperatingSystem");
			OperatingSystemObject = osSearcher.Get().OfType<ManagementObject>().SingleOrDefault();

			IsInitialized = true;

			logger.LogInformation("Metrics collector was initialized successfully");

			return Task.CompletedTask;
		}

		public Task<MetricsBag> CollectMetrics(CancellationToken cancellationToken)
		{
			if (!IsInitialized)
			{
				throw new InvalidOperationException("Metrics collector is not initialized");
			}

			var metrics = new MetricsBag(settings.AgentId, GetMetrics());

			return Task.FromResult(metrics);
		}

		private IEnumerable<MetricsValue> GetMetrics()
		{
			if (TotalCpuObject != null)
			{
				TotalCpuObject.Get();
				yield return new MetricsValue
				{
					Id = "cpu",
					Timestamp = DateTimeOffset.UtcNow,
					Value = (ulong)TotalCpuObject["PercentProcessorTime"],
				};
			}

			if (OperatingSystemObject != null)
			{
				OperatingSystemObject.Get();

				var free = (ulong)OperatingSystemObject["FreePhysicalMemory"];
				var total = (ulong)OperatingSystemObject["TotalVisibleMemorySize"];
				var used = total - free;

				yield return new MetricsValue
				{
					Id = "memory",
					Timestamp = DateTimeOffset.UtcNow,
					Value = used,
				};
			}
		}
	}
}
