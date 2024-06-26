﻿using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OData.Edm;
using OdataToEntity.AspNetCore;
using OdataToEntity.EfCore.DynamicDataContext;
using OdataToEntity.EfCore.DynamicDataContext.InformationSchema;

namespace Project_OdataToEntity
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            // If using Kestrel:
            services.Configure<KestrelServerOptions>(options =>
            {

                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {

                options.AllowSynchronousIO = true;
            });
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(
               options => options.SetIsOriginAllowed(x => _ = true).AllowAnyMethod().AllowAnyHeader().AllowCredentials()
              );

            String basePath = Configuration.GetValue<String>("OdataToEntity:BasePath");
            String provider = Configuration.GetValue<String>("OdataToEntity:Provider");
            String connectionString = Configuration.GetValue<String>("OdataToEntity:ConnectionString");
            bool useRelationalNulls = Configuration.GetValue<bool>("OdataToEntity:UseRelationalNulls");
            //String? informationSchemaMappingFileName = Configuration.GetValue<String>("OdataToEntity:InformationSchemaMappingFileName");
            //String? filter = Configuration.GetValue<String>("OdataToEntity:Filter");
            //String? defaultSchema = Configuration.GetSection("OdataToEntity:DefaultSchema").Get<String>();
            //String[]? includedSchemas = Configuration.GetSection("OdataToEntity:IncludedSchemas").Get<String[]>();
            //String[]? excludedSchemas = Configuration.GetSection("OdataToEntity:ExcludedSchemas").Get<String[]>();

            if (!String.IsNullOrEmpty(basePath) && basePath[0] != '/')
                basePath = "/" + basePath;

            InformationSchemaSettings informationSchemaSettings = null;// new InformationSchemaSettings();
            //if (!String.IsNullOrEmpty(defaultSchema))
            //    informationSchemaSettings.DefaultSchema = defaultSchema;
            //if (includedSchemas != null)
            //    informationSchemaSettings.IncludedSchemas = new HashSet<String>(includedSchemas);
            //if (excludedSchemas != null)
            //    informationSchemaSettings.ExcludedSchemas = new HashSet<String>(excludedSchemas);
            //if (filter != null)
            //    informationSchemaSettings.ObjectFilter = Enum.Parse<DbObjectFilter>(filter, true);
            //if (informationSchemaMappingFileName != null)
            //{
            //    String json = File.ReadAllText(informationSchemaMappingFileName);
            //    var informationSchemaMapping = System.Text.Json.JsonSerializer.Deserialize<InformationSchemaMapping>(json)!;
            //    informationSchemaSettings.Operations = informationSchemaMapping.Operations;
            //    informationSchemaSettings.Tables = informationSchemaMapping.Tables;
            //}

            var schemaFactory = new DynamicSchemaFactory(provider, connectionString);
            using (ProviderSpecificSchema providerSchema = schemaFactory.CreateSchema(useRelationalNulls))
            {
                IEdmModel edmModel = DynamicMiddlewareHelper.CreateEdmModel(providerSchema, informationSchemaSettings);

                app.UseOdataToEntityMiddleware<OePageMiddleware>(basePath, edmModel);

            }

        }
    }
}
