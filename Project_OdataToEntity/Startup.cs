using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OData.Edm;
using OdataToEntity.AspNetCore;
using OdataToEntity.EfCore.DynamicDataContext;
using OdataToEntity.EfCore.DynamicDataContext.InformationSchema;
using Serilog;
using Serilog.AspNetCore;
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
            services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .WriteTo.File("./logs/log-.txt",rollingInterval:RollingInterval.Day));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging();
            app.UseCors(
               options => options.SetIsOriginAllowed(x => _ = true).AllowAnyMethod().AllowAnyHeader().AllowCredentials()
              );

            String basePath = Configuration.GetValue<String>("OdataToEntity:BasePath");
            String provider = Configuration.GetValue<String>("OdataToEntity:Provider");
            String connectionString = Configuration.GetValue<String>("OdataToEntity:ConnectionString");
            bool useRelationalNulls = Configuration.GetValue<bool>("OdataToEntity:UseRelationalNulls");

            
            var schemaFactory = new DynamicSchemaFactory(provider, connectionString);
            using (ProviderSpecificSchema providerSchema = schemaFactory.CreateSchema(useRelationalNulls))
            {
                IEdmModel edmModel = DynamicMiddlewareHelper.CreateEdmModel(providerSchema, null);

                basePath?.Split(',').ToList().ForEach(x =>
                {
                    app.UseOdataToEntityMiddleware<OePageMiddleware>(new PathString(x), edmModel);
                });


            }

        }
    }
}
