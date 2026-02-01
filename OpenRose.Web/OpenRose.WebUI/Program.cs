// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudExtensions.Services;
using OpenRose.WebUI.Client.Services.BaselineHierarchy;
using OpenRose.WebUI.Client.Services.BaselineItemz;
using OpenRose.WebUI.Client.Services.BaselineItemzCollection;
using OpenRose.WebUI.Client.Services.BaselineItemzTrace;
using OpenRose.WebUI.Client.Services.BaselineItemzTypes;
using OpenRose.WebUI.Client.Services.Baselines;
using OpenRose.WebUI.Client.Services.Export;
using OpenRose.WebUI.Client.Services.GoTo;
using OpenRose.WebUI.Client.Services.Hierarchy;
using OpenRose.WebUI.Client.Services.Import;
using OpenRose.WebUI.Client.Services.Itemz;
using OpenRose.WebUI.Client.Services.ItemzChangeHistory;
using OpenRose.WebUI.Client.Services.ItemzCollection;
using OpenRose.WebUI.Client.Services.ItemzTrace;
using OpenRose.WebUI.Client.Services.ItemzType;
using OpenRose.WebUI.Client.Services.ItemzTypeItemzsService;
using OpenRose.WebUI.Client.Services.Project;
using OpenRose.WebUI.Components;
using OpenRose.WebUI.Components.EventServices;
using OpenRose.WebUI.Components.FindServices;
using OpenRose.WebUI.Configuration;
using OpenRose.WebUI.Services;
using System.Reflection;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Configuration.AddEnvironmentVariables(); // Add environment variables to configuration

// Configure API settings
builder.Services.Configure<APISettings>(builder.Configuration.GetSection("ApiSettings"));

// Retrieve API settings
var apiSettings = builder.Configuration.GetSection("APISettings").Get<APISettings>();


var configurationService = new ConfigurationService
{
    IsOpenRoseAPIConfigured = !string.IsNullOrEmpty(apiSettings?.BaseUrl)
};

builder.Services.AddSingleton(configurationService);


// --- NEW: perform an API version check when a BaseUrl is provided ---
if (configurationService.IsOpenRoseAPIConfigured)
{
	try
	{
		// Get the WebUI informational version (same approach as AssemblyInfoService)
		var webUiVersion = Assembly.GetExecutingAssembly()
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

		// Short timeout to avoid long startup delays
		using var http = new HttpClient
		{
			BaseAddress = new Uri(apiSettings!.BaseUrl),
			Timeout = TimeSpan.FromSeconds(5)
		};

		var resp = http.GetAsync("api/version").GetAwaiter().GetResult();
		if (!resp.IsSuccessStatusCode)
		{
			configurationService.IsOpenRoseAPIConfigured = false;
			configurationService.ApiVersionMismatchMessage =
				$"Unable to contact OpenRose API at '{apiSettings.BaseUrl}'. Please ensure the API is running and reachable.";
			var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
			logger.LogError("API version endpoint returned status {Status}. Marking API as unavailable.", resp.StatusCode);
		}
		else
		{
			// Read JSON and get InformationalVersion
			var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;
			var apiVersion = root.GetProperty("informationalVersion").GetString() ?? "";

			configurationService.ApiVersion = apiVersion;
			configurationService.WebUiVersion = webUiVersion;

			if (!string.Equals(apiVersion, webUiVersion, StringComparison.OrdinalIgnoreCase))
			{
				configurationService.IsOpenRoseAPIConfigured = false; // force UI to show error
				configurationService.ApiVersionMismatchMessage =
					$"OpenRose API version ({apiVersion}) does not match this OpenRose WebUI version ({webUiVersion}). " +
					"To avoid possible data corruption, please update both components to the same release. " +
					"Contact your system administrator for assistance.";
				var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
				logger.LogError("API/WebUI version mismatch. API: {ApiVersion}, WebUI: {WebUiVersion}", apiVersion, webUiVersion);
			}
			else
			{
				// versions match; keep configured = true
				var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
				logger.LogInformation("API and WebUI version match: {Version}", apiVersion);
			}
		}
	}
	catch (Exception ex)
	{
		configurationService.IsOpenRoseAPIConfigured = false;
		configurationService.ApiVersionMismatchMessage =
			$"Error while checking OpenRose API version: {ex.Message}";
		var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "Exception while checking API version");
	}
}
// --- END new version check ---

if (!configurationService.IsOpenRoseAPIConfigured)
{
    var configFile = string.IsNullOrEmpty(builder.Environment.EnvironmentName) ? "appsettings.json" : $"appsettings.{builder.Environment.EnvironmentName}.json";
    builder.Logging.AddConsole();
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogError($"OpenRose API connection via base URL is not configured. Please contact your System Administrator.");

    // Register dummy implementations that return errors or mock data
    builder.Services.AddSingleton<IProjectService, DummyProjectService>();
    builder.Services.AddSingleton<IItemzTypeService, DummyItemzTypeService>();
    builder.Services.AddSingleton<IItemzTypeItemzsService, DummyItemzTypeItemzsService>();
    builder.Services.AddSingleton<IItemzService, DummyItemzService>();
    builder.Services.AddSingleton<IHierarchyService, DummyHierarchyService>();
    builder.Services.AddSingleton<IItemzTraceService, DummyItemzTraceService>();
    builder.Services.AddSingleton<IItemzCollectionService, DummyItemzCollectionService>();
    builder.Services.AddSingleton<IItemzChangeHistoryService, DummyItemzChangeHistoryService>();
    builder.Services.AddSingleton<IBaselinesService, DummyBaselinesService>();
    builder.Services.AddSingleton<IBaselineHierarchyService, DummyBaselineHierarchyService>();
    builder.Services.AddSingleton<IBaselineItemzTypesService, DummyBaselineItemzTypesService>();
    builder.Services.AddSingleton<IBaselineItemzService, DummyBaselineItemzService>();
    builder.Services.AddSingleton<IBaselineItemzTraceService, DummyBaselineItemzTraceService>();
    builder.Services.AddSingleton<IBaselineItemzCollectionService, DummyBaselineItemzCollectionService>();
    builder.Services.AddSingleton<IExportService, DummyExportService>();
	builder.Services.AddSingleton<IImportService, DummyImportService>();
}
else
{

    builder.Services.AddHttpClient<IProjectService, ProjectService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IItemzTypeService, ItemzTypeService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IItemzTypeItemzsService, ItemzTypeItemzsService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IItemzService, ItemzService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IHierarchyService, HierarchyService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IItemzTraceService, ItemzTraceService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IItemzCollectionService, ItemzCollectionService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IItemzChangeHistoryService, ItemzChangeHistoryService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IBaselinesService, BaselinesService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IBaselineHierarchyService, BaselineHierarchyService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IBaselineItemzTypesService, BaselineItemzTypesService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IBaselineItemzService, BaselineItemzService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IBaselineItemzTraceService, BaselineItemzTraceService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });

    builder.Services.AddHttpClient<IBaselineItemzCollectionService, BaselineItemzCollectionService>(client =>
    {
        client.BaseAddress = new Uri(apiSettings!.BaseUrl);
    });
	builder.Services.AddHttpClient<IExportService, ExportService>(client =>
	{
		client.BaseAddress = new Uri(apiSettings!.BaseUrl);
	});
	builder.Services.AddHttpClient<IImportService, ImportService>(client =>
	{
		client.BaseAddress = new Uri(apiSettings!.BaseUrl);
	});
	builder.Services.AddHttpClient<IGoToService, GoToService>(client =>
	{
		client.BaseAddress = new Uri(apiSettings!.BaseUrl);
	});


	// EXPLAINATION :: "FindProjectAndBaselineIdsByBaselineItemzIdService" depens on "IBaselineHierarchyService" and so 
	// we need to register it here within the else block where we know that OpenRose API settings are available.
	builder.Services.AddScoped<IFindProjectAndBaselineIdsByBaselineItemzIdService, FindProjectAndBaselineIdsByBaselineItemzIdService>();
}

builder.Services.AddMudServices();
builder.Services.AddMudExtensions();

builder.Services.AddScoped<TreeNodeItemzSelectionService>(); // Register the service
builder.Services.AddScoped<BaselineTreeNodeItemzSelectionService>(); // Register the service
builder.Services.AddScoped<BaselineBreadcrumsService>(); // Register the service
builder.Services.AddScoped<BreadcrumsService>(); // Register the service
builder.Services.AddScoped<FormStateService>(); // Register the service
builder.Services.AddScoped<ViewSettingsService>(); // Register the ReadOnlyView toggle service


builder.Services.AddSingleton<AssemblyInfoService>(); // Register the service

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OpenRose.WebUI.Client._Imports).Assembly);

app.Run();
