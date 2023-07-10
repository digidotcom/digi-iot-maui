using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace BLEMicrocontrollerSample
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
					handlers.AddHandler(typeof(BLEMicrocontrollerSample.Pages.MainPage), typeof(BLEMicrocontrollerSample.iOS.ContentPageHandler));
					handlers.AddHandler(typeof(BLEMicrocontrollerSample.Pages.SendFilePage), typeof(BLEMicrocontrollerSample.iOS.ContentPageHandler));
#endif
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}