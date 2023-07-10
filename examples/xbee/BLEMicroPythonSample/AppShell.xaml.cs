using BLEMicroPythonSample.ViewModels;

namespace BLEMicroPythonSample
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		protected override bool OnBackButtonPressed()
		{
			Dispatcher.Dispatch(async () =>
			{
				if (Current.CurrentPage.BindingContext is ViewModelBase)
				{
					await ((ViewModelBase)Current.CurrentPage.BindingContext).NavigateBack();
				}
			});
			return true;
		}
	}
}