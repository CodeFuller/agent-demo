using CodeFuller.Library.Bootstrap;
using CodeFuller.Library.Logging;
using DemoAgent.Interfaces;
using DemoAgent.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DemoAgent
{
	internal class ApplicationBootstrapper : BasicApplicationBootstrapper<IApplicationLogic>
	{
		protected override void RegisterServices(IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<AgentSettings>(configuration.Bind);

			services.AddTransient<IMetricsCollector, MetricsCollector>();
			services.AddSingleton<IApplicationLogic, ApplicationLogic>();
		}

		protected override void BootstrapLogging(ILoggerFactory loggerFactory, IConfiguration configuration)
		{
			loggerFactory.AddLogging(settings => configuration.Bind("logging", settings));
		}
	}
}
