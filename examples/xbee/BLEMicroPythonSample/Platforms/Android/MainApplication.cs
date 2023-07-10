using Acr.UserDialogs;
using Android.App;
using Android.Runtime;

namespace BLEMicroPythonSample
{
	[Application]
	public class MainApplication : MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
			UserDialogs.Init(this);
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}