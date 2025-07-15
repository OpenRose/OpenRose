// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;
using ItemzApp.API.BusinessRules.ItemzType;
using ItemzApp.API.BusinessRules.Project;
using ItemzApp.API.BusinessRules.Baseline;
using ItemzApp.API.DbContexts;
using ItemzApp.API.DbContexts.Interceptors;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Serilog;
using Serilog.Events;
using OpenRose.API.Services;

namespace ItemzApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(setupAction =>
                {
                    setupAction.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
                   setupAction.SerializerSettings.Converters.Add(new StringEnumConverter());
                   setupAction.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    
                })               
               
                .AddMvcOptions(setupAction =>
                {
                    // EXPLANATION: By including AddMvcOptions within controller, we are able to 
                    // configure global attribute values for StatusCodes which are expected to be 
                    // returned by every Controller in this API project. This is one way to resolve
                    // issue related to explicitly typing this information at Controller's class level
                    // in every controller. This setting is very useful for generating Swagger Documents 
                    // and Swagger UI with good documentations.

                    setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
                    setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
                    setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
                    // EXPLANATION: with "ProduceAttribute" filter we can configure that our API can 
                    // return back Json as well as XML and tihs information is used by Swagger to produce
                    // necessary docs and UI. But we have to configure the OutputFormatters

                    setupAction.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
                    setupAction.OutputFormatters.Add(new Microsoft.AspNetCore.Mvc.Formatters.XmlSerializerOutputFormatter());
                    
                    // EXPLANATION: With ReturnHttpNotAcceptable set to true, when consumer asks for 
                    // unsupported media type then we return back http 406 error indicating that requested
                    // media type is not supported. If we don't set this value then by default it would send
                    // responce back in configured default media type (in our case it would be "application/json")
                    setupAction.ReturnHttpNotAcceptable = true;

                    // EXPLANATION: With ConsumesAttribute, we specify media type in which consumer of the API
                    // has to call different http endpoints. This will help improve Swagger Documentation

//                    setupAction.InputFormatters.Add(item: new Microsoft.AspNetCore.Mvc.Formatters.XmlSerializerInputFormatter(new MvcOptions()));
                    setupAction.Filters.Add(new ConsumesAttribute("application/json"));
                })
                .ConfigureApiBehaviorOptions(setupAction =>
                {
                    setupAction.InvalidModelStateResponseFactory = context =>
                    {
                        // here we adjust problem details item
                        var problemDetailsFactory = context.HttpContext.RequestServices
                            .GetRequiredService<ProblemDetailsFactory>();
                        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                            context.HttpContext,
                            context.ModelState,
                            detail: "See the error Field for details.",
                            instance: context.HttpContext.Request.Path);

                        // There are some information which is not passed back to the 
                        // API consumer as part of Problem Details. This is what we are 
                        // now populating in the below properties.

                        // problemDetails.Detail = "See the error Field for details.";
                        // problemDetails.Instance = context.HttpContext.Request.Path;

                        // find out which status code to use

                        var actionExecutingContext =
                            context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                        // if there are modelstate errors & all arguments were correctly 
                        // found/parsed we're dealing with validation errors
                        //if ((context.ModelState.ErrorCount > 0) &&
                        //(actionExecutingContext?.ActionArguments.Count ==
                        //context.ActionDescriptor.Parameters.Count))
                        // Decided to modify IF condition just check number of Errors in ModelState.

                        if (context.ModelState.ErrorCount > 0)
                        {
                            problemDetails.Type = "http://MYWEBSITENAME.COM/modelvalidationproblem"; // TODO : I'm just using MYWEBSITENAME here as we don't have any for now.
                            problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                            problemDetails.Title = "One or more validation error occured.";

                            return new UnprocessableEntityObjectResult(problemDetails)
                            {
                                ContentTypes = { "application/problem+json" }
                            };
                        };

                        // if one of the aruguments wasn't correctly found / couldn't be parsed
                        // we're dealing with null/unparseable input

                        problemDetails.Status = StatusCodes.Status400BadRequest;
                        problemDetails.Title = "One or more errors on input occured.";
                        return new BadRequestObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<IItemzRepository, ItemzRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IItemzTypeRepository, ItemzTypeRepository>();
            services.AddScoped<IBaselineRepository, BaselineRepository>();
            services.AddScoped<IBaselineItemzRepository, BaselineItemzRepository>();
            services.AddScoped<IBaselineItemzTypeRepository, BaselineItemzTypeRepository>();
            services.AddScoped<IProjectRules,ProjectRules>();
            services.AddScoped<IItemzTypeRules, ItemzTypeRules>();
            services.AddScoped<IBaselineRules, BaselineRules>();
            services.AddScoped<IItemzChangeHistoryRepository, ItemzChangeHistoryRepository>();
            services.AddScoped<IItemzChangeHistoryByItemzTypeRepository, ItemzChangeHistoryByItemzTypeRepository>();
            services.AddScoped<IItemzChangeHistoryByProjectRepository, ItemzChangeHistoryByProjectRepository>();
            services.AddScoped<IItemzChangeHistoryByRepositoryRepository, ItemzChangeHistoryByRepositoryRepository>();
            services.AddScoped<IItemzTraceRepository, ItemzTraceRepository>();
            services.AddScoped<IBaselineItemzTraceRepository, BaselineItemzTraceRepository>();
            services.AddScoped<IHierarchyRepository, HierarchyRepository>();
            services.AddScoped<IBaselineHierarchyRepository, BaselineHierarchyRepository>();
			services.AddScoped<IExportNodeMapper, ExportNodeMapper>();
			services.AddScoped<IItemzTraceExportService, ItemzTraceExportService>();
            services.AddScoped<IBaselineItemzTraceExportService, BaselineItemzTraceExportService>();
			services.AddScoped<IImportService, ImportService>();

			// EXPLANATION: As described in the Blog Article, https://purple.telstra.com/blog/a-better-way-of-resolving-ef-core-interceptors-with-dependency-injection
			// we are now registering ItemzContextInterceptor in the DI Container as Singleton service 
			// and then utilizing it in AddDbContext method to register this Interceptor via provider.GetRequiredService.
			// This way, just in case, if an Interceptor needs other services from the DI Container like logger, etc. then
			// it will be possible to include them in the constructor parameter of the Interceptor.

			services.AddScoped<ItemzContexInterceptor>();

			var connectionString = Configuration.GetConnectionString("ItemzContext");

			services.AddDbContext<ItemzContext>((serviceProvider, options) =>
            {
				options.UseSqlServer(
                    connectionString: connectionString,
                                        builder => builder.UseHierarchyId())
                    .AddInterceptors(serviceProvider.GetRequiredService<ItemzContexInterceptor>()); 
            });

            services.AddDbContext<ItemzChangeHistoryContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(
				connectionString: connectionString,
                                        builder => builder.UseHierarchyId());
            });

            services.AddDbContext<BaselineContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(
                connectionString: connectionString,
                                        builder => builder.UseHierarchyId());
            });

            services.AddDbContext<ItemzTraceContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(
                connectionString: connectionString,
                                        builder => builder.UseHierarchyId());
            });

            services.AddDbContext<BaselineItemzTraceContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(
                connectionString: connectionString,
                                        builder => builder.UseHierarchyId());
            });
          
            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("ItemzApp.OpenAPI.Specification",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "ItemzApp OpenAPI",
                        Version = "0.1",
                        Description = "TODO describe my API",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email = "TODO type in correct email address",
                            Name = "TODO type in name of the company owning this software",
                            Url = new Uri("https://TODO-COMPANY-WEBSITE-URL")
                        },
                        License = new Microsoft.OpenApi.Models.OpenApiLicense()
                        {
                            Name = "TODO decide on license type",
                            Url = new Uri("https://TODO-PROVIDE-LICENSE-URL/")
                        },
                        TermsOfService = new Uri("https://TODO-TERMS-OF-SERVICE-URL/")
                    });
                var autogeneratedXMLFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var autogeneratedXMLFullPath = Path.Combine(AppContext.BaseDirectory, autogeneratedXMLFileName);
                setupAction.IncludeXmlComments(autogeneratedXMLFullPath);
			});
            services.AddSwaggerGenNewtonsoftSupport(); // required for generating ENUM as string in Swagger Docs. Ref: https://stackoverflow.com/a/59833198

			// TODO: As part of planned upgrade to ASP .NET Core 5.x, we shall utilize
			// new feature that allows DB Errors to be integrated into 'app.UseDeveloperExceptionPage()' within Configure method.
			// Checkout https://docs.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/5.0/middleware-database-error-page-obsolete#recommended-action

			services.AddHostedService<KeepSQLConnectionAliveService>();

			// This below can be done to allow KeepSQLConnectionAliveService to be injected from DI container in the application
			//  services.AddSingleton<IKeepSQLConnectionAliveService, KeepSQLConnectionAliveService>(); 
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, ItemzContext itemzContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSerilogRequestLogging(options =>
                {
                    // options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms; User: {@User}";
                    // options.IncludeQueryInRequestPath = true;
                    options.GetLevel = (ctx, _, ex) =>
                    {
                        if (ex != null || ctx.Response.StatusCode > 499)
                        {
                            return LogEventLevel.Error;
                        }

                        // ignore health check endpoints
                        if (ctx.Request.Path.StartsWithSegments("/health"))
                        {
                            return LogEventLevel.Debug;
                        }

                        return LogEventLevel.Information;
                    };
                });
                logger.LogInformation("Current Process ID : {0}", Process.GetCurrentProcess().Id);
            }
            else
            {
                // TODO: UseExceptionHandler is used for adding middleware to the pipeline
                // for catching exception. I have to learn more about it to understand middleware
                // and pipeline concepts of ASP.NET Core.

                // this code is going to handle all server side exception and will show custom message
                // in client application when environment is set anything other then development.
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happend. Try again later. If error continues then contact this Application Administrator.");
                    });
                });
            }
            app.UseStaticFiles();
            
            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(setupAction.RoutePrefix) ? "." : "..";
                setupAction.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/ItemzApp.OpenAPI.Specification/swagger.json",
                    "ItemzApp OpenAPI");
                setupAction.RoutePrefix = string.Empty; // RoutePrefix will set the root of the application to be SwaggerUI
                setupAction.EnableDeepLinking(); // Allows URL to contain path to Tags and Operations
                setupAction.DisplayOperationId(); // Shows Opeartion ID against each Operation.
            });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });



            // EXPLANATION :: At the starting point of the application, we are going to check if
            // Repository entry is there in the DB for ItemzHierarchy table. If it's not there
            // then we will insert a new entry. This is important to uniquely identify repository. 

            var insertRepositoryEntryInDatabase = new InsertRepositoryEntryInDatabase(itemzContext);
            insertRepositoryEntryInDatabase.EnsureRepositoryEntryInDatabaseExists();
            insertRepositoryEntryInDatabase = null;
        }
    }
}
