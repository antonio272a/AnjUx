using AnjUx.Client.Services;
using AnjUx.Client.Stores;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace AnjUx.Client
{
    public static class CoreRegistor
    {
        public static void Register(IServiceCollection services)
        {
            services.AddSingleton<CoreNavBarStore>();

            // EditorFor
            services.AddScoped<EditorForService>();

            // Loader
            services.AddSingleton<LoadingService>();

            services.AddScoped<CoreNotificationService>();
            services.AddScoped<CoreDialogService>();

            // Radzen Dialog, Notification, ContextMenu and ToolTip components
            services.AddScoped<DialogService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<TooltipService>();
            services.AddScoped<ContextMenuService>();
        }
    }
}
