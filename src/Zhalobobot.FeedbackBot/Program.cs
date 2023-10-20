using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting;
using Vostok.Hosting.AspNetCore.Houston;
using Vostok.Hosting.AspNetCore.Houston.Applications;
using Vostok.Hosting.AspNetCore.Web;
using Vostok.Hosting.Houston;
using Vostok.Hosting.Houston.Abstractions;
using Vostok.Hosting.Houston.Configuration;
using Vostok.Hosting.Setup;
using Zhalobobot.Bot;

// namespace Zhalobobot.Bot
// {
//     public class Program
//     {
//         public static void Main(string[] args)
//         {
//             CreateHostBuilder(args).Build().Run();
//         }
//
//         public static IHostBuilder CreateHostBuilder(string[] args) =>
//             Host.CreateDefaultBuilder(args)
//                 .ConfigureWebHostDefaults(webBuilder =>
//                 {
//                     webBuilder.UseStartup<Startup>();
//                 });
//     }
// }

[assembly: HoustonEntryPoint(typeof(BotApplication))]

await new HoustonHost(new BotApplication(), ConfigureHost)
    .WithConsoleCancellation()
    .RunAsync();

static void ConfigureHost(IHostingConfiguration config)
{
    config.OutOfHouston.SetupEnvironment(EnvironmentSetup);
}

static void EnvironmentSetup(IVostokHostingEnvironmentBuilder builder)
{
    builder
        .SetupApplicationIdentity(identityBuilder => identityBuilder
            .SetProject("stukov")
            .SetApplication("Zhalobobot")
            .SetEnvironment("cloud"))
        .SetupLog(logBuilder => logBuilder
            .SetupConsoleLog());
}