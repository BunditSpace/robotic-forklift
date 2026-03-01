using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forklift.Infrastructure.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Seeds the database by ensuring it is created. This method can be extended to include additional seeding logic if necessary.
        /// </summary>
        /// <param name="app">The host application.</param>
        /// <returns>The host application.</returns>
        public static IHost SeedDatabase(this IHost app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    context.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<AppDbContext>>();
                    logger.LogError(ex, "An error occurred while creating the database.");
                    throw;
                }
            }
            return app;
        }
    }
}
