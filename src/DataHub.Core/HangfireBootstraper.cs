using DataHub.Core.Database;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DataHub.Core
{
    public static class HangfireBootstraper
    {
        public static void AddHangfireMongo(this IServiceCollection services, string mongoConnectionString)
        {
            var mongoUrl = new MongoUrl(mongoConnectionString);

            services.AddHangfire(configuration =>
            {
                configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseColouredConsoleLogProvider()
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings();

                configuration.UseMongoStorage(mongoUrl.Url, mongoUrl.DatabaseName, new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        BackupPostfix = "backup",
                        BackupStrategy = MongoBackupStrategy.Database,
                        Strategy = MongoMigrationStrategy.Migrate,
                    }
                });
            });

            services.AddTransient<IMongoDbContext>(serviceProvider =>
            {
                var mongoUrl = new MongoUrl(mongoConnectionString);
                return new MongoDbContext(mongoUrl.Url, mongoUrl.DatabaseName);
            });
        }
    }
}
