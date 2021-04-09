using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CodeFuller.Library.Configuration;
using CodeFuller.Library.Logging;
using DemoAgent.Interfaces;
using DemoAgent.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DemoAgent
{
	public static class Program
	{
		public static async Task<int> Main(string[] args)
		{
			try
			{
				await CreateHostBuilder(args).Build().RunAsync();
				return 0;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				Console.Error.WriteLine(e);
				return e.HResult;
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			// By default Windows service will have c:\Windows\System32 as the current directory.
			// As result logs directory will be created in wrong location.
			Directory.SetCurrentDirectory(GetAppDirectory());

			return Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(configurationBuilder => configurationBuilder.LoadSettings("conf", args))
				.ConfigureLogging((hostContext, loggingBuilder) =>
				{
					loggingBuilder.ClearProviders();
					loggingBuilder.AddLogging(settings => hostContext.Configuration.Bind("logging", settings));
				})
				.ConfigureServices((hostContext, services) => ConfigureServices(services, hostContext.Configuration))
				.UseWindowsService();
		}

		private static string GetAppDirectory()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<AgentSettings>(configuration.Bind);

			services.AddTransient<IMetricsCollector, MetricsCollector>();
			services.AddHostedService<AgentService>();
		}
	}
}
