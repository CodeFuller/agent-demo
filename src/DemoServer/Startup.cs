using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DemoServer
{
	public class Startup
	{
		private readonly IConfiguration configuration;

		public Startup(IConfiguration configuration)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

#pragma warning disable CA1801 // Remove unused parameter
		public void ConfigureServices(IServiceCollection services)
#pragma warning restore CA1801 // Remove unused parameter
		{
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			var webSocketOptions = new WebSocketOptions
			{
				KeepAliveInterval = TimeSpan.FromSeconds(120),
			};

			app.UseWebSockets(webSocketOptions);

			app.Use(async (context, next) =>
			{
				if (context.Request.Path == "/ws")
				{
					if (context.WebSockets.IsWebSocketRequest)
					{
						var webSocket = await context.WebSockets.AcceptWebSocketAsync();
						await Echo(webSocket, context.RequestAborted);
					}
					else
					{
						context.Response.StatusCode = 400;
					}
				}
				else
				{
					await next();
				}
			});
		}

		private static async Task Echo(WebSocket webSocket, CancellationToken cancellationToken)
		{
			var buffer = new byte[4 * 1024];

			while (true)
			{
				var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
				if (receiveResult.CloseStatus.HasValue)
				{
					await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, cancellationToken);
					break;
				}

				var responseData = ProcessRequest(buffer);
				await webSocket.SendAsync(responseData, receiveResult.MessageType, receiveResult.EndOfMessage, cancellationToken);
			}
		}

		private static ArraySegment<byte> ProcessRequest(ArraySegment<byte> requestData)
		{
			var inputData = Encoding.UTF8.GetString(requestData);
			var reversedData = ReverseString(inputData);

			var responseData = Encoding.UTF8.GetBytes(reversedData);
			return new ArraySegment<byte>(responseData, 0, responseData.Length);
		}

		public static string ReverseString(string s)
		{
			var charArray = s.ToCharArray();
			Array.Reverse(charArray);
			return new String(charArray);
		}
	}
}
