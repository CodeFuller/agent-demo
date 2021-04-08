using System.Threading;
using System.Threading.Tasks;
using AgentDemo.Common;

namespace DemoAgent.Interfaces
{
	public interface IMetricsCollector
	{
		Task<MetricsBag> CollectMetrics(CancellationToken cancellationToken);
	}
}
