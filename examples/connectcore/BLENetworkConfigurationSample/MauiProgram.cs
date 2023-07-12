using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace BLENetworkConfigurationSample
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
					handlers.AddHandler(typeof(BLENetworkConfigurationSample.Pages.MainPage), typeof(BLENetworkConfigurationSample.iOS.ContentPageHandler));
					handlers.AddHandler(typeof(BLENetworkConfigurationSample.Pages.DevicePage), typeof(BLENetworkConfigurationSample.iOS.ContentPageHandler));
					handlers.AddHandler(typeof(BLENetworkConfigurationSample.Pages.SettingsPage), typeof(BLENetworkConfigurationSample.iOS.ContentPageHandler));
#endif
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}