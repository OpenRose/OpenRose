// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ItemzApp.API.DbContexts;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ItemzApp.API.DbContexts
{
    public class ItemzContextFactory : IDesignTimeDbContextFactory<ItemzContext>
    {
		// TODO: REMOVE THIS CODE AS THIS WAS SOMETHING I WAS TRYING OUT FROM THE VIDEO
		// THAT I WAS LEARNING ABOUT IDesignTimeDbContextFactory<TContext>. WE ARE NOT GOING
		// TO USE THIS OPTION FOR NOW BUT WE COULD CONSIDER USING IT IN THE FUTURE.
		//
		// Later I found "https://blog.tonysneed.com/2018/12/20/idesigntimedbcontextfactory-and-dependency-injection-a-love-story/"
		// blog post which has clean code and examples on how to use ASP.NET Core Configuration System + 
		// Environment Variable + Dependency Injection to implement IDesignTimeDbContextFactory<TContext>
		//
		// There is one more blog that describes when and how to use IDesignTimeDbContextFactory<TContext>
		// which can be found at ... https://snede.net/you-dont-need-a-idesigntimedbcontextfactory/

		public ItemzContext CreateDbContext(string[] args)
        {
			// Determine the environment
			string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

			// Build configuration
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{environment}.json", optional: true)
				.Build();

			var optionsBuilder = new DbContextOptionsBuilder<ItemzContext>();
			var connectionString = configuration.GetConnectionString("ItemzContext");

            optionsBuilder.UseSqlServer(connectionString, 
                                            options => options.EnableRetryOnFailure()
                                                .CommandTimeout(100)
                                                .UseHierarchyId() 
                );
            Console.WriteLine(connectionString);
            return new ItemzContext(optionsBuilder.Options);
        }
    }
}
