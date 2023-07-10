/*
 * Copyright 2023, Digi International Inc.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using BleMicroPythonSample.ViewModels;
using BLEMicroPythonSample.Models;

namespace BLEMicroPythonSample.Pages;

public partial class TemperaturePage : ContentPage
{
	// Variables.
	private readonly TemperaturePageViewModel temperaturePageViewModel;

	public TemperaturePage(BleDevice device)
	{
		InitializeComponent();
		temperaturePageViewModel = new TemperaturePageViewModel(device);
		BindingContext = temperaturePageViewModel;

		// Initialize the refresh rate picker.
		List<string> rates = new()
		{
			"1 second",
			"2 seconds",
			"5 seconds",
			"10 seconds",
			"30 seconds",
			"60 seconds"
		};
		ratePicker.ItemsSource = rates;
		ratePicker.SelectedIndex = 2;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		temperaturePageViewModel.RegisterEventHandler();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		temperaturePageViewModel.UnregisterEventHandler();
	}

	public async void StartButtonClicked(object sender, System.EventArgs e)
	{
		bool newState = !temperaturePageViewModel.IsRunning;

		// Change the text of the button.
		startButton.Text = newState ? "Stop" : "Start";
		// Enable or disable the rate picker.
		ratePicker.IsEnabled = !newState;
		// Start or stop the process.
		bool success = newState ? await temperaturePageViewModel.StartProcess(int.Parse(((string)ratePicker.SelectedItem).Split(" ")[0]))
			: await temperaturePageViewModel.StopProcess();
		// If the process has started or stopped successfully, change the opacity of the grid.
		if (success)
			dataGrid.Opacity = newState ? 1 : 0.2;
	}
}