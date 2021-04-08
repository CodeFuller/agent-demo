using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentDemo.Common;
using DemoAgent.Interfaces;
using Microsoft.Extensions.Options;

namespace DemoAgent.Internal
{
	internal class MetricsCollector : IMetricsCollector
	{
		private readonly Random rnd = new();

		private readonly AgentSettings settings;

		public MetricsCollector(IOptions<AgentSettings> options)
		{
			this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public Task<MetricsBag> CollectMetrics(CancellationToken cancellationToken)
		{
			var metrics = new MetricsBag(settings.AgentId, GetMetrics());

			return Task.FromResult(metrics);
		}

		// TODO: Replace with real system metrics.
		private IEnumerable<MetricsValue> GetMetrics()
		{
			var timestamp = DateTimeOffset.UtcNow;

			yield return new MetricsValue
			{
				Id = "cpu",
				Timestamp = timestamp,
				Value = GetRandomMetricsValue(),
			};

			yield return new MetricsValue
			{
				Id = "memory",
				Timestamp = timestamp,
				Value = GetRandomMetricsValue(),
			};
		}

		private double GetRandomMetricsValue()
		{
#pragma warning disable CA5394 // Do not use insecure randomness
			return rnd.NextDouble();
#pragma warning restore CA5394 // Do not use insecure randomness
		}
	}
}
