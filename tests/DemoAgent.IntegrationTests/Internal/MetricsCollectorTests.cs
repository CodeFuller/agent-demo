using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DemoAgent.Internal;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock;

namespace DemoAgent.IntegrationTests.Internal
{
	[TestClass]
	public class MetricsCollectorTests
	{
		[TestMethod]
		public async Task CollectMetrics_ReturnsAllMetrics()
		{
			// Arrange

			var settings = new AgentSettings
			{
				AgentId = "Some Agent",
			};

			var mocker = new AutoMocker();
			mocker.Use(Options.Create(settings));
			var target = mocker.CreateInstance<MetricsCollector>();

			// Act

			await target.Initialize(CancellationToken.None);
			var metricsBag = await target.CollectMetrics(CancellationToken.None);

			// Assert

			metricsBag.AgentId.Should().Be("Some Agent");

			var metrics = metricsBag.Metrics.ToList();

			metrics.Should().HaveCount(2);

			metrics[0].Id.Should().Be("cpu");
			metrics[0].Value.Should().BeInRange(0, 100);

			metrics[1].Id.Should().Be("memory");
		}
	}
}
