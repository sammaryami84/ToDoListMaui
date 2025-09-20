using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;


namespace MainToDoList
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionCore()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseLocalNotification(config =>
                {
                    config.AddAndroid(android =>
                    {
                        // کانالی با صدای سفارشی (alarm.mp3 در res/raw)
                        android.AddChannel(new NotificationChannelRequest
                        {
                            Id = "reminders",
                            Name = "Task Reminders",
                            Description = "Reminder notifications with custom sound",
                            Importance = AndroidImportance.Max,   // برای صدادار بودن الزامی
                            EnableVibration = true,
                            EnableLights = true,
                            EnableSound = true,
                            Sound = "alarm",                      // بدون پسوند
                            LockScreenVisibility = AndroidVisibilityType.Public
                        });
                    });
                });


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}