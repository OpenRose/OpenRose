// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.Services.AddScoped<IProjectService, ProjectService>();
//builder.Services.AddHttpClient<IProjectService, ProjectService>(client =>
//{
//    //client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
//	client.BaseAddress = new Uri("http://localhost:51087");
//});

await builder.Build().RunAsync();
