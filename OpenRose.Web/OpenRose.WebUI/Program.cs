// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
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


builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Configuration.AddEnvironmentVariables(); // Add environment variables to configuration


var startupCapabilities = new StartupCapabilitiesService(builder.Configuration);
builder.Services.AddSingleton(startupCapabilities);

//if (startupCapabilities.ApiAvailable)
//{
//	// Configure API settings
//	builder.Services.Configure<APISettings>(builder.Configuration.GetSection("ApiSettings"));
//}

if (startupCapabilities.ServerOfflineAvailable)
{
	// EXPLANATION: Bind OfflineContent settings from appsettings.json.
	// This config controls where server-side JSON files are stored and how offline mode behaves.
	builder.Services.Configure<OfflineContentSettings>(builder.Configuration.GetSection("OfflineContent"));

	builder.Services.AddHttpClient("WebUIInternal", (sp, client) =>
	{
		var context = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
		if (context is not null)
		{
			var request = context.Request;
			var baseUri = $"{request.Scheme}://{request.Host}";
			client.BaseAddress = new Uri(baseUri);
		}
	});
}



////// EXPLANATION : Initialize the shared APIConfigurationService singleton.
////// This service acts as a central "state container" for API connection status and version info.
////// We use SetConnectionState() instead of setting properties directly because:
//////   1. Properties have private setters to enforce consistency.
//////   2. Every update triggers NotifyStateChanged(), which raises the OnChange event.
//////      That event tells all Blazor components to re-render automatically when the API state changes.
////// Here we set the initial state based on whether the API BaseUrl is configured in appsettings.
////var apiConfigurationService = new APIConfigurationService();
////apiConfigurationService.SetConnectionState(
////	isOpenRoseAPIConfigured: !string.IsNullOrEmpty(apiSettings?.BaseUrl),
////	apiVersion: null,
////	message: null);
////builder.Services.AddSingleton(apiConfigurationService);

var apiConfigurationService = new APIConfigurationService();
apiConfigurationService.SetConnectionState(
	isOpenRoseAPIConfigured: startupCapabilities.ApiAvailable,
	apiVersion: null,
	message: null
);
builder.Services.AddSingleton(apiConfigurationService);


//// --- NEW: register HttpClient for version check + background monitor ---
//if (startupCapabilities.ApiAvailable)
//{

//	// Mark API as configured so UI does NOT collapse
//	apiConfigurationService.SetConnectionState(
//		isOpenRoseAPIConfigured: true,
//		apiVersion: null,
//		message: null
//	);
//	// Get the WebUI informational version (same approach as AssemblyInfoService)
//	var webUiVersion = Assembly.GetExecutingAssembly()
//		.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

//	apiConfigurationService.WebUiVersion = webUiVersion;

//	builder.Services.AddHttpClient("VersionCheck", client =>
//	{
//		client.BaseAddress = new Uri(apiSettings!.BaseUrl);
//		client.Timeout = TimeSpan.FromSeconds(5);
//	});

//	// Register the shared checker
//	builder.Services.AddSingleton<ApiVersionChecker>();

//	// Register both hosted services
//	//builder.Services.AddHostedService<ApiVersionMonitorService>();
//	builder.Services.AddHostedService<ApiConnectionWatcherService>();
//}
//// --- END new version check ---


if (!startupCapabilities.ApiAvailable)
{
    // var configFile = string.IsNullOrEmpty(builder.Environment.EnvironmentName) ? "appsettings.json" : $"appsettings.{builder.Environment.EnvironmentName}.json";
    // builder.Logging.AddConsole();
    // var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    // logger.LogError($"OpenRose API connection via base URL is not configured. Please contact your System Administrator.");

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

	// Retrieve API settings
	var apiSettings = builder.Configuration.GetSection("APISettings").Get<APISettings>();

	#region register HttpClient for version check + background monitor

	//// Mark API as configured so UI does NOT collapse
	//apiConfigurationService.SetConnectionState(
	//	isOpenRoseAPIConfigured: true,
	//	apiVersion: null,
	//	message: null
	//);

	// Get the WebUI informational version (same approach as AssemblyInfoService)
	var webUiVersion = Assembly.GetExecutingAssembly()
		.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

	apiConfigurationService.WebUiVersion = webUiVersion;

	builder.Services.AddHttpClient("VersionCheck", client =>
	{
		client.BaseAddress = new Uri(apiSettings!.BaseUrl);
		client.Timeout = TimeSpan.FromSeconds(5);
	});

	// Register the shared checker
	builder.Services.AddSingleton<ApiVersionChecker>();

	// Register both hosted services
	//builder.Services.AddHostedService<ApiVersionMonitorService>();
	builder.Services.AddHostedService<ApiConnectionWatcherService>();

	#endregion


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


	//builder.Services.AddHttpContextAccessor();



	// EXPLAINATION :: "FindProjectAndBaselineIdsByBaselineItemzIdService" depens on "IBaselineHierarchyService" and so 
	// we need to register it here within the else block where we know that OpenRose API settings are available.
	builder.Services.AddScoped<IFindProjectAndBaselineIdsByBaselineItemzIdService, FindProjectAndBaselineIdsByBaselineItemzIdService>();
	builder.Services.AddScoped<IFindProjectOfItemzId, FindProjectOfItemzId>();

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
builder.Services.AddScoped<TreeNodeItemzSelectionServiceForJson>(); // Register the service


builder.Services.AddScoped<AssemblyInfoService>(); // Register the service

// EXPLANATION: Register server-side offline catalog repository.
// This service manages JSON files stored in the OfflineContent.StorageFolder.
builder.Services.AddScoped<OfflineCatalogRepository>();

//// EXPLANATION: Register the startup resolver that determines whether to start in API or Offline JSON mode.
//builder.Services.AddScoped<OfflineStartupResolver>();

// builder.Services.AddSingleton<StartupCapabilitiesService>();

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
	.AddAdditionalAssemblies(typeof(OpenRose.WebUI.Client._Imports).Assembly);

//.AddInteractiveWebAssemblyRenderMode()

//// EXPLANATION:
//// Run the startup resolver to determine whether to start in API mode or Offline JSON mode.
//// This must run AFTER DI is built but BEFORE the app begins handling requests.
//using (var scope = app.Services.CreateScope())
//{
//	var resolver = scope.ServiceProvider.GetRequiredService<OfflineStartupResolver>();
//	var result = await resolver.ResolveStartupModeAsync();

//	var dataSourceState = scope.ServiceProvider.GetRequiredService<DataSourceStateService>();
//	var viewSettings = scope.ServiceProvider.GetRequiredService<ViewSettingsService>();

//	if (result == OfflineStartupResolver.StartupResult.OfflineMode)
//	{
//		viewSettings.IsOperatingInJsonFileDataSourceMode = true;
//	}
//	else
//	{
//		viewSettings.IsOperatingInJsonFileDataSourceMode = false;
//	}
//}

//////------------------------------------------------------------
//////FIX OPTION A IMPLEMENTATION
////// Run OfflineStartupResolver ONCE when the application starts.
////// This prevents it from running on every Blazor circuit creation,
//////navigation, or reconnection.
////// ------------------------------------------------------------
////app.Lifetime.ApplicationStarted.Register(() =>
////{
////	_ = Task.Run(async () =>
////	{
////		using var scope = app.Services.CreateScope();

////var resolver = scope.ServiceProvider.GetRequiredService<OfflineStartupResolver>();
////var result = await resolver.ResolveStartupModeAsync();

////var dataSourceState = scope.ServiceProvider.GetRequiredService<DataSourceStateService>();
////var viewSettings = scope.ServiceProvider.GetRequiredService<ViewSettingsService>();
////var nav = scope.ServiceProvider.GetRequiredService<NavigationManager>();

////switch (result)
////{
////	case OfflineStartupResolver.StartupResult.ApiMode:
////		dataSourceState.SwitchToApiDataSource();
////		viewSettings.IsOperatingInJsonFileDataSourceMode = false;

////		Stay on Home("/") — no navigation needed
////				break;

////	case OfflineStartupResolver.StartupResult.OfflineMode:
////		viewSettings.IsOperatingInJsonFileDataSourceMode = true;

////		JSON file was successfully loaded->go to JSON viewer
////				nav.NavigateTo("/jsonviewer", forceLoad: false);
////		break;

////	case OfflineStartupResolver.StartupResult.Error:
////	default:
////		dataSourceState.InitializeToNone();
////		viewSettings.IsOperatingInJsonFileDataSourceMode = false;

////		Stay on Home("/") — no navigation needed
////				break;
////}
////	});
////});




app.MapControllers();

app.Run();
