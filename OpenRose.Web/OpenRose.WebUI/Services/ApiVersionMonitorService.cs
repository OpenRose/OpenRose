// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;

//namespace OpenRose.WebUI.Services
//{

//	public class ApiVersionMonitorService : BackgroundService
//	{
//		private readonly ApiVersionChecker _checker;
//		private readonly ConfigurationService _config;
//		private readonly ILogger<ApiVersionMonitorService> _logger;

//		public ApiVersionMonitorService(ApiVersionChecker checker,
//										ConfigurationService config,
//										ILogger<ApiVersionMonitorService> logger)
//		{
//			_checker = checker;
//			_config = config;
//			_logger = logger;
//		}

//		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//		{
//			while (!stoppingToken.IsCancellationRequested)
//			{
//				var healthy = await _checker.CheckApiVersionAsync(stoppingToken);

//				if (healthy)
//				{
//					// Clear error message once healthy
//					_config.SetConnectionState(
//						isConfigured: true,
//						apiVersion: _config.ApiVersion, // keep current version if already known
//						message: string.Empty           // clear error
//					);

//					// Pause monitoring while healthy
//					_logger.LogInformation("API/WebUI versions compatible. Pausing monitor until connection is lost.");

//					// Sleep for a long time, but still allow cancellation
//					await Task.Delay(Timeout.Infinite, stoppingToken);
//				}
//				else
//				{
//					// Retry after 2 minutes if unhealthy
//					await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
//				}
//			}
//		}
//	}
//}


