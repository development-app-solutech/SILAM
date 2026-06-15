using Microsoft.EntityFrameworkCore;

namespace ProlabWeb.Extensions
{
    public static class HostExtensions
    {
        public static async Task<IHost> CreateOrMigrateDbContextAsync<TContext>(this IHost host, Func<TContext, IServiceProvider, Task> seeder) where TContext : DbContext
        {

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var logger = services.GetRequiredService<ILogger<TContext>>();

                var context = services.GetService<TContext>();
                
                if (context == null)
                {
                    throw new InvalidOperationException($"Unable to resolve service for type {typeof(TContext).Name}");
                }

                if (seeder == null)
                {
                    throw new ArgumentNullException(nameof(seeder));
                }

                try
                {
                    logger.LogInformation("Migrating Or Creating database associated with context {DbContextName}", typeof(TContext).Name);

                    CreateOrMigrate(context, services);

                    logger.LogInformation("Seeding database associated with context {DbContextName}", typeof(TContext).Name);

                    await InvokeSeeder(seeder, context, services);

                    logger.LogInformation("Migrated or created database associated with context {DbContextName}", typeof(TContext).Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);

                    throw;

                }
            }

            return host;
        }

        private static void CreateOrMigrate<TContext>(TContext context, IServiceProvider services)
            where TContext : DbContext
        {
            if (context.Database.GetMigrations().Any())
            {
                context.Database.Migrate();
            }
            else
            {
                context.Database.EnsureCreated();
            }
        }

        private static Task InvokeSeeder<TContext>(Func<TContext, IServiceProvider, Task> seeder, TContext context, IServiceProvider services)
            where TContext : DbContext
        {
            return seeder(context, services);
        }
    }
} 