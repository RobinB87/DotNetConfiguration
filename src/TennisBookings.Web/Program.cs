using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TennisBookings.Web.Data;

namespace TennisBookings.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var hostingEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
                var appLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();

                if (hostingEnvironment.IsDevelopment())
                {
                    var ctx = serviceProvider.GetRequiredService<TennisBookingDbContext>();
                    await ctx.Database.MigrateAsync(appLifetime.ApplicationStopping);

                    try
                    {
                        var userManager = serviceProvider.GetRequiredService<UserManager<TennisBookingsUser>>();
                        var roleManager = serviceProvider.GetRequiredService<RoleManager<TennisBookingsRole>>();

                        await SeedData.SeedUsersAndRoles(userManager, roleManager);
                    }
                    catch (Exception ex)
                    {
                        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("UserInitialisation");
                        logger.LogError(ex, "Failed to seed user data");
                    }
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    //
                    // To be able to use Azure KV:
                    // First ensure Visual Studio is logged in with the same account as Azure
                    // If not sufficient; install and authenticate Azure CLI on machine
                    // Still not sufficient: configure Azure Cloud Shell (shell.azure.com) -> create storage
                    //

                    // During DEV can use temporary API key
                    // But for PROD use ctx.HostingEnvironment.IsProduction
                    // By combining user-secrets for local DEV and Azure KV for PROD: ensure no keys are exposed
                    if (ctx.HostingEnvironment.IsProduction())
                    {
                        // build current config, so we can access one of the required keys
                        // is temporary instance of IConfiguration, is thrown away after method finishes
                        var config = builder.Build();

                        var tokenProvider = new AzureServiceTokenProvider();

                        // create new KeyVaultClient which requires an AuthenticationCallbackDelegate
                        // here KeyVaultTokenCallback is used
                        var kvClient = new KeyVaultClient((authority, resource, scope) =>
                            tokenProvider.KeyVaultTokenCallback(authority, resource, scope));

                        // DefaultKeyVaultSecretManager translates the name used in Azure KV (replaces double dashes -- by a colon separator :)
                        builder.AddAzureKeyVault(config["KeyVault:BaseUrl"], kvClient,
                            new DefaultKeyVaultSecretManager());
                    }

                    // First configure dev machine with AWS secret and access key as env variables
                    // AWS SDK will pick this up automagically
                    // Follow guide: https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/aws-sdk-net-dg.pdf#quick-start

                    // cd to \src\TennisBookings.Web:
                    // set AWS_PROFILE = <profilename>
                    // set AWS_REGION = <region name as in appsettings>

                    // TODO: Fix, does not work yet
                    //builder.AddSystemsManager("/tennisBookings");
                });
    }
}
