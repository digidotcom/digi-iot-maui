using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace BLEConfigurationSample
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers((handlers) =>
                {
#if IOS
                    handlers.AddHandler(typeof(BLEConfigurationSample.Pages.MainPage), typeof(BLEConfigurationSample.iOS.ContentPageHandler));
                    handlers.AddHandler(typeof(BLEConfigurationSample.Pages.ConfigurationPage), typeof(BLEConfigurationSample.iOS.ContentPageHandler));
#endif
                });

#if DEBUG
		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}