﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;
using Library.API.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Diagnostics;
using NLog.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Serialization;

namespace Library.API
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("dbConnections.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            

            services.AddMvc(setupAction => {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
            })
            .AddJsonOptions(options => {
                options.SerializerSettings.ContractResolver = 
                    new CamelCasePropertyNamesContractResolver();
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            //var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            //services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));
            
            var sqliteConnectionString = Configuration["connectionStrings:SQLite"];
            var mySQLConnectionString = Configuration["connectionStrings:MySQL"];
            services.AddDbContext<LibraryContext>(
                options => options.UseSqlite(sqliteConnectionString)
                //options => options.UseMySql(mySQLConnectionString)
            );

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext =
                    implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            }
            
            );

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<ITypeHelperService, TypeHelperService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug(LogLevel.Information);
            //loggerFactory.AddProvider(new NLog.Extensions.Logging.NLogLoggerProvider());
            loggerFactory.AddNLog();


            if (env.IsDevelopment())
            {
				app.UseDeveloperExceptionPage();

            }
            else
            {
                app.UseExceptionHandler(appBuilder => {
                    appBuilder.Run(async context => {
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if(exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                        }


                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Unexpected fault. Please try again later.");
                    });
                });
            }


            AutoMapper.Mapper.Initialize(config => {
                config.CreateMap<Entities.Author, Models.AuthorDto>()
                    .ForMember(destination => destination.Name, builder => builder.MapFrom(
                        source => $"{source.FirstName} {source.LastName}"
                    ))
                    .ForMember(destination => destination.Age, builder => builder.MapFrom(
                        source => source.DateOfBirth.GetCurrentAge()
                    ));
                config.CreateMap<Models.AuthorForCreationDto, Entities.Author>();
                config.CreateMap<Entities.Book, Models.BookDto>();
                config.CreateMap<Models.BookForCreationDto, Entities.Book>();
                config.CreateMap<Models.BookForUpdateDto, Entities.Book>();
                config.CreateMap<Entities.Book, Models.BookForUpdateDto>();

            });


            libraryContext.EnsureSeedDataForContext();

            app.UseMvc();
        }
    }
}
