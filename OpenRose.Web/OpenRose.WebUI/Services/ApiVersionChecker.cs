// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenRose.WebUI.Configuration;

namespace OpenRose.WebUI.Services
{

	public class ApiVersionChecker
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly APIConfigurationService _apiConfigService;
		private readonly ILogger<ApiVersionChecker> _logger;

		public ApiVersionChecker(IHttpClientFactory httpClientFactory,
								 APIConfigurationService config,
								 ILogger<ApiVersionChecker> logger)
		{
			_httpClientFactory = httpClientFactory;
			_apiConfigService = config;
			_logger = logger;
		}

		public async Task<bool> CheckApiVersionAsync(CancellationToken token = default)
		{
			try
			{
				var client = _httpClientFactory.CreateClient("VersionCheck");
				var resp = await client.GetAsync("api/version", token);

				if (!resp.IsSuccessStatusCode)
				{

					_apiConfigService.SetConnectionState(
						isOpenRoseAPIConfigured:false, 
						apiVersion:null, 
						message: $"Unable to contact OpenRose API. Status: {resp.StatusCode}"
						);

					_logger.LogError("API version endpoint returned {Status}", resp.StatusCode);
					return false;
				}

				var json = await resp.Content.ReadAsStringAsync(token);
				using var doc = JsonDocument.Parse(json);
				var apiVersion = doc.RootElement.GetProperty("informationalVersion").GetString() ?? "";


				if (!string.Equals(apiVersion, _apiConfigService.WebUiVersion, StringComparison.OrdinalIgnoreCase))
				{

					_apiConfigService.SetConnectionState(
						isOpenRoseAPIConfigured: false,
						apiVersion: apiVersion,
						message: $"OpenRose API version ({apiVersion}) does not match WebUI version ({_apiConfigService.WebUiVersion})."
						);

					_logger.LogError("Version mismatch. API: {ApiVersion}, WebUI: {WebUiVersion}", apiVersion, _apiConfigService.WebUiVersion);
					return false;
				}
				else
				{

					_apiConfigService.SetConnectionState(
						isOpenRoseAPIConfigured: true,
						apiVersion: apiVersion,
						message: string.Empty // ✅ clear error
						);


					_logger.LogInformation("API/WebUI versions match: {Version}", apiVersion);
					return true;
				}
			}
			catch (Exception ex)
			{

				_apiConfigService.SetConnectionState(
					isOpenRoseAPIConfigured: false,
					apiVersion: null,
					message: $"Exception during API version check : No connection could be made because the target machine actively refused it."
					);


				_logger.LogError("Exception during API version check : No connection could be made because the target machine actively refused it.");
				return false;
			}
		}
	}

}
