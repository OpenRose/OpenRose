// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ItemzApp.API.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ItemzApp.API
{
	public class Program
	{
		public static void Main(string[] args)
		{

			// EXPLANATION: first lets build configuration that can be then used for Logger.
			// We are doing it in the Main public static void main method because we want it to 
			// start logging right from the main method itself. Otherwise we could have leveraged
			// Dependency Injection of ASP.NET Core to create logger later on.

			var configuration = new ConfigurationBuilder()
			   .SetBasePath(Directory.GetCurrentDirectory())
			   .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
			   .Build();


			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.Enrich.FromLogContext()
				.CreateLogger();

			try
			{
				Log.Information("Starting Up");


				var host = CreateHostBuilder(args).Build();

				//var logger = host.Services.GetRequiredService<ILogger<Program>>(); // Program in code 'GetRequiredService<ILogger<Program>>' indicates Logging Category.

				// migrate the database. Best Practice = in main, using service scope

				// EXPLAINED: All about ASP .NET Core start-up is explained very well by Andrew Lock in this
				// blog article and blog series...
				// https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
				// We are utilizing '4. Manually running tasks in Program.cs' subtopic from above blog article
				// for database set-up when it comes to initializing database with test data everytime
				// application starts-up.

				using (var scope = host.Services.CreateScope())
				{
					try
					{
						var context = scope.ServiceProvider.GetService<ItemzContext>();
						// for demo purposes, delete the database & migrate on startup so 
						// we can start with a clean slate
						// context?.Database.EnsureDeletedzzzzzzzzzzz();
						context?.Database.Migrate();
						Log.Information("Database deleted >>> migrated >>> seeded completed");
					}
					catch (Exception ex)
					{
						////var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); 
						//logger.LogError(ex, "An error occurred while migrating the database.");
						Log.Fatal(ex, "An error occurred while migrating the database.");
					}
				}

				//logger.LogInformation("Host created.");
				Log.Information("Application Started...");
				host.Run();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Application start-up failed");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog()

				// Based on following Blogpost, we added UseDefaultServiceProvider 
				// within CreateHostBuilder method.
				// https://weblogs.asp.net/ricardoperes/asp-net-core-pitfalls-dependency-injection-lifetime-validation

				.UseDefaultServiceProvider((context, options) =>
				{
					options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
					options.ValidateOnBuild = true;
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
