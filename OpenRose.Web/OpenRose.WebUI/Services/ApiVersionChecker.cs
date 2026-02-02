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
		private readonly ConfigurationService _config;
		private readonly ILogger<ApiVersionChecker> _logger;

		public ApiVersionChecker(IHttpClientFactory httpClientFactory,
								 ConfigurationService config,
								 ILogger<ApiVersionChecker> logger)
		{
			_httpClientFactory = httpClientFactory;
			_config = config;
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
					_config.IsOpenRoseAPIConfigured = false;
					_config.ApiVersionMismatchMessage =
						$"Unable to contact OpenRose API. Status: {resp.StatusCode}";
					_logger.LogError("API version endpoint returned {Status}", resp.StatusCode);
					return false;
				}

				var json = await resp.Content.ReadAsStringAsync(token);
				using var doc = JsonDocument.Parse(json);
				var apiVersion = doc.RootElement.GetProperty("informationalVersion").GetString() ?? "";

				_config.ApiVersion = apiVersion;

				if (!string.Equals(apiVersion, _config.WebUiVersion, StringComparison.OrdinalIgnoreCase))
				{
					_config.IsOpenRoseAPIConfigured = false;
					_config.ApiVersionMismatchMessage =
						$"OpenRose API version ({apiVersion}) does not match WebUI version ({_config.WebUiVersion}).";
					_logger.LogError("Version mismatch. API: {ApiVersion}, WebUI: {WebUiVersion}", apiVersion, _config.WebUiVersion);
					return false;
				}
				else
				{
					_config.IsOpenRoseAPIConfigured = true;
					_config.ApiVersionMismatchMessage = string.Empty; // ✅ clear error
					_logger.LogInformation("API/WebUI versions match: {Version}", apiVersion);
					return true;
				}
			}
			catch (Exception ex)
			{
				_config.IsOpenRoseAPIConfigured = false;
				_config.ApiVersionMismatchMessage = $"Error checking API: {ex.Message}";
				_logger.LogError(ex, "Exception during API version check");
				return false;
			}
		}
	}

}
