// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.Project;
using OpenRose.WebUI.Client.Services.ItemzType;
using OpenRose.WebUI.Client.Services.ItemzTypeItemzsService;
using OpenRose.WebUI.Client.Services.Itemz;
using OpenRose.WebUI.Client.Services.Hierarchy;
using OpenRose.WebUI.Client.Services.ItemzCollection;
using OpenRose.WebUI.Client.Services.ItemzChangeHistory;
using OpenRose.WebUI.Client.Services.ItemzTrace;
using OpenRose.WebUI.Client.Services.Baselines;
using OpenRose.WebUI.Client.Services.BaselineHierarchy;
using OpenRose.WebUI.Client.Services.BaselineItemzTypes;
using OpenRose.WebUI.Client.Services.BaselineItemz;
using OpenRose.WebUI.Client.Services.BaselineItemzCollection;
using OpenRose.WebUI.Client.Services.BaselineItemzTrace;
using OpenRose.WebUI.Components;
using OpenRose.WebUI.Components.EventServices;
using OpenRose.WebUI.Components.FindServices;
using OpenRose.WebUI.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudExtensions.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpClient<IProjectService, ProjectService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IItemzTypeService, ItemzTypeService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IItemzTypeItemzsService, ItemzTypeItemzsService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IItemzService, ItemzService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IHierarchyService, HierarchyService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IItemzTraceService, ItemzTraceService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IItemzCollectionService, ItemzCollectionService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IItemzChangeHistoryService, ItemzChangeHistoryService>(client =>
{
client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IBaselinesService, BaselinesService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IBaselineHierarchyService, BaselineHierarchyService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IBaselineItemzTypesService, BaselineItemzTypesService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IBaselineItemzService, BaselineItemzService>(client =>
{
client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IBaselineItemzTraceService, BaselineItemzTraceService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddHttpClient<IBaselineItemzCollectionService, BaselineItemzCollectionService>(client =>
{
client.BaseAddress = new Uri("http://localhost:51087");
});

builder.Services.AddMudServices();
builder.Services.AddMudExtensions();

builder.Services.AddScoped<TreeNodeItemzSelectionService>(); // Register the service
builder.Services.AddScoped<BaselineTreeNodeItemzSelectionService>(); // Register the service
builder.Services.AddScoped<BaselineBreadcrumsService>(); // Register the service
builder.Services.AddScoped<BreadcrumsService>(); // Register the service
builder.Services.AddScoped<IFindProjectAndBaselineIdsByBaselineItemzIdService, FindProjectAndBaselineIdsByBaselineItemzIdService>();

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

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OpenRose.WebUI.Client._Imports).Assembly);

app.Run();
