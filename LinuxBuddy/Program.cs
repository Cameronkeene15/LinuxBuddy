using Microsoft.Extensions.DependencyInjection;
using LinuxBuddy.Services;

namespace LinuxBuddy
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            // Register your services
            services.AddSingleton<SettingsService>();
            services.AddSingleton<ChatService>();
            services.AddSingleton<LinuxBuddyApp>();

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the app and run
            var app = serviceProvider.GetRequiredService<LinuxBuddyApp>();
            await app.RunAsync(args);
        }
    }
}