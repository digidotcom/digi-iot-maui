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

using BLENetworkConfigurationSample.Models;
using BLENetworkConfigurationSample.ViewModels;

namespace BLENetworkConfigurationSample.Pages;

public partial class MainPage : ContentPage
{
	// Variables.
	private readonly MainPageViewModel mainPageViewModel;

	private bool initializing = false;

	public MainPage()
	{
		InitializeComponent();
		mainPageViewModel = new MainPageViewModel();
		BindingContext = mainPageViewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		mainPageViewModel.ActivePage = true;

		await Task.Run(() =>
		{
			if (initializing)
			{
				return;
			}
			initializing = true;

			// Verify that Bluetooth and GPS interfaces as well as the necessary services are enabled.
			if (MainPageViewModel.CheckBluetooth())
			{
				mainPageViewModel.SubscribeBluetoothListeners();

				// Start looking for BLE devices.
				mainPageViewModel.StartScan();
				initializing = false;
			}
			else
			{
				mainPageViewModel.DisableControls();
				initializing = false;
				return;
			}
		});
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		mainPageViewModel.ActivePage = false;

		mainPageViewModel.StopScan();
	}

	/// <summary>
	/// Method called when a list item is selected.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">Event args.</param>
	public void OnItemSelected(object sender, SelectionChangedEventArgs e)
	{
		// Clear selection.
		((CollectionView)sender).SelectedItem = null;

		if (((CollectionView)sender).SelectedItem is not BleDevice selectedDevice || !selectedDevice.IsActive)
			return;

		// Prepare the connection.
		mainPageViewModel.PrepareConnection(selectedDevice);
	}
}