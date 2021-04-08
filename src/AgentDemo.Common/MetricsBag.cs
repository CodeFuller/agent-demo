using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentDemo.Common
{
	public class MetricsBag
	{
		public string AgentId { get; init; }

		public IReadOnlyCollection<MetricsValue> Metrics { get; init; }

		public MetricsBag()
		{
		}

		public MetricsBag(string agentId, IEnumerable<MetricsValue> metrics)
		{
			if (String.IsNullOrWhiteSpace(agentId))
			{
				throw new ArgumentException("Agent id is missing", nameof(agentId));
			}

			AgentId = agentId;
			Metrics = metrics?.ToList() ?? throw new ArgumentNullException(nameof(metrics));
		}
	}
}
