using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ItemzApp.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace OpenRose.API.Services
{
	public class KeepSQLConnectionAliveService : BackgroundService, IKeepSQLConnectionAliveService
	{
		private readonly ILogger<KeepSQLConnectionAliveService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly TimeSpan _interval;

		public KeepSQLConnectionAliveService(ILogger<KeepSQLConnectionAliveService> logger, IServiceProvider serviceProvider, IConfiguration configuration)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			var intervalInSeconds = configuration.GetValue<int>("KeepSQLConnectionAliveService:IntervalInSeconds");
			_interval = TimeSpan.FromSeconds(intervalInSeconds);
		}

		public async Task SendKeepAliveAsync(CancellationToken stoppingToken)
		{
			_logger.LogDebug("Sending keep-alive query to SQL Server.");
			using (var scope = _serviceProvider.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<ItemzContext>();
				try
				{
					await context.Database.ExecuteSqlRawAsync("SELECT 1", stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error sending keep-alive query to SQL Server.");
				}
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				await SendKeepAliveAsync(stoppingToken);
				await Task.Delay(_interval, stoppingToken);
			}
		}
	}
}