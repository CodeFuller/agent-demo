﻿using System;
using System.Threading.Tasks;
using AgentDemo.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace DemoServer
{
	public class MetricsHub : Hub
	{
		private readonly ILogger<MetricsHub> logger;

		public MetricsHub(ILogger<MetricsHub> logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task DumpMetrics(MetricsBag metrics)
		{
			logger.LogInformation("Received metrics: {@Metrics}", metrics);

			var request = new ServerToClientRequest
			{
				Timestamp = DateTimeOffset.Now,
			};

			logger.LogInformation("Sending request to client ...");
			await Clients.Caller.SendAsync("ServerToClientCall", request, Context.ConnectionAborted);
		}
	}
}
