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
using InterfacesConfigurationSample.ViewModels;

namespace BLENetworkConfigurationSample.Pages;

public partial class DevicePage : ContentPage
{
	// Variables.
	private readonly BleDevice bleDevice;

	/// <summary>
	/// Class constructor. Instantiates a new <c>DevicePage</c> 
	/// object with the provided parameters.
	/// </summary>
	/// <param name="bleDevice">The BLE device to represent in the page.</param>
	public DevicePage(BleDevice bleDevice)
	{
		this.bleDevice = bleDevice;

		InitializeComponent();
		BindingContext = new DevicePageViewModel(bleDevice);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext == null)
		{
			return;
		}
	}
}