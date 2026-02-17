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
using OpenRose.WebUI.Client.Services.JsonFile;
using OpenRose.WebUI.Client.Services.Project;
using OpenRose.WebUI.Components;
using OpenRose.WebUI.Components.EventServices;
using OpenRose.WebUI.Components.FindServices;
using OpenRose.WebUI.Configuration;
using OpenRose.WebUI.Services;
using System.Reflection;
using System.Runtime.InteropServices;
//using System.Reflection;
//using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);


// Detect if running on Windows
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
	builder.Host.UseWindowsService(); // Run as Windows Service
}
else
{
	builder.Host.UseConsoleLifetime(); // Normal console app for Linux/macOS
}



// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Configuration.AddEnvironmentVariables(); // Add environment variables to configuration

// Configure API settings
builder.Services.Configure<APISettings>(builder.Configuration.GetSection("ApiSettings"));

// Retrieve API settings
var apiSettings = builder.Configuration.GetSection("APISettings").Get<APISettings>();


// EXPLANATION : Initialize the shared ConfigurationService singleton.
// This service acts as a central "state container" for API connection status and version info.
// We use SetConnectionState() instead of setting properties directly because:
//   1. Properties have private setters to enforce consistency.
//   2. Every update triggers NotifyStateChanged(), which raises the OnChange event.
//      That event tells all Blazor components to re-render automatically when the API state changes.
// Here we set the initial state based on whether the API BaseUrl is configured in appsettings.
var configurationService = new ConfigurationService();
configurationService.SetConnectionState(
	isConfigured: !string.IsNullOrEmpty(apiSettings?.BaseUrl),
	apiVersion: null,
	message: null);
builder.Services.AddSingleton(configurationService);


// --- NEW: register HttpClient for version check + background monitor ---
if (configurationService.IsOpenRoseAPIConfigured)
{
	// Get the WebUI informational version (same approach as AssemblyInfoService)
	var webUiVersion = Assembly.GetExecutingAssembly()
		.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

	configurationService.WebUiVersion = webUiVersion;

	builder.Services.AddHttpClient("VersionCheck", client =>
	{
		client.BaseAddress = new Uri(apiSettings!.BaseUrl);
		client.Timeout = TimeSpan.FromSeconds(5);
	});

	// Register the shared checker
	builder.Services.AddSingleton<ApiVersionChecker>();

	// Register both hosted services
	builder.Services.AddHostedService<ApiVersionMonitorService>();
	builder.Services.AddHostedService<ApiConnectionWatcherService>();

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
builder.Services.AddScoped<DataSourceStateService>(); // This service tracks whether we're using API or JSON file as data source
builder.Services.AddScoped<JsonFileSchemaValidationService>(); // This service validates JSON files against the OpenRose export schema
builder.Services.AddScoped<JsonFileDataSourceService>(); // This service provides hierarchy/project data queries from loaded JSON files
builder.Services.AddScoped<BaselineTreeNodeItemzSelectionServiceForJson>(); // Register the service



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
