using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace X_Server.Extensions
{
    public static class ApplicationManagerExtension
    {
        public static void AddApplicationManager(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ApplicationManager.Storage.Configuration>(x => config.Bind("Storage", x));
            services.Configure<ApplicationManager.Downloader.Models.Applications>(x => config.Bind("Storage:Applications", x));

            services.AddSingleton<ApplicationManager.Tasker.Manager>();
            services.AddSingleton<ApplicationManager.Downloader.Manager>();
            services.AddSingleton<ApplicationManager.Storage.Manager>();
        }
    }
}
