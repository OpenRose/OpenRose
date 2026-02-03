// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenRose.WebUI.Configuration;

namespace OpenRose.WebUI.Services
{
	public class ApiConnectionWatcherService : BackgroundService
	{
		private readonly ApiVersionChecker _checker;
		private readonly ConfigurationService _config;
		private readonly ILogger<ApiConnectionWatcherService> _logger;

		public ApiConnectionWatcherService(ApiVersionChecker checker,
										   ConfigurationService config,
										   ILogger<ApiConnectionWatcherService> logger)
		{
			_checker = checker;
			_config = config;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var healthy = await _checker.CheckApiVersionAsync(stoppingToken);

				if (!healthy)
				{
					_logger.LogWarning("API connection lost. Will retry...");
				}

				await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
			}
		}
	}
}
