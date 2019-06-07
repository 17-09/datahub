using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DataHub.Server
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
            var mongoUrl = new MongoUrl(Configuration.GetConnectionString("Mongo"));

            services.AddHangfire(configuration =>
            {
                configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseColouredConsoleLogProvider()
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseMongoStorage(mongoUrl.Url, mongoUrl.DatabaseName, new MongoStorageOptions
                    {
                        MigrationOptions = new MongoMigrationOptions
                        {
                            BackupPostfix = "backup",
                            BackupStrategy = MongoBackupStrategy.Database,
                            Strategy = MongoMigrationStrategy.Migrate,
                        }
                    });
            });
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseHangfireServer();
            app.UseHangfireDashboard();
        }
    }
}
