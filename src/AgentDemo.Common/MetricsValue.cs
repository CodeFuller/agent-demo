using System;

namespace AgentDemo.Common
{
	public class MetricsValue
	{
		public string Id { get; init; }

		public DateTimeOffset Timestamp { get; init; }

		public double Value { get; init; }
	}
}
