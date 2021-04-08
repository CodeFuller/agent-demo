using System;

namespace DemoAgent
{
	internal class AgentSettings
	{
		public string AgentId { get; set; }

		public Uri ServerAddress { get; set; }

		public TimeSpan MetricsUpdateInterval { get; set; } = TimeSpan.FromSeconds(5);
	}
}
