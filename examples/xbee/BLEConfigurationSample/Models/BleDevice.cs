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

using BLEConfigurationSample.Utils;
using DigiIoT.Maui.Devices.XBee;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace BLEConfigurationSample
{
	public class BleDevice : INotifyPropertyChanged
	{
		// Constants.
		public static readonly string MAC_REGEX = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
		public static readonly string MAC_REPLACE = "$1:$2:$3:$4:$5:$6";

		public static readonly int INACTIVITY_TIME = 10000;

		// Variables.
		private int rssi = RssiUtils.WORST_RSSI;

		private bool isActive = true;

		// Properties.
		/// <summary>
		/// Gets the internal BLE device object associated to this device.
		/// </summary>
		public IDevice Device { get; private set; }

		/// <summary>
		/// Gets the BLE ID of this device.
		/// </summary>
		public Guid Id => Device.Id;

		/// <summary>
		/// Gets the BLE name of this device.
		/// </summary>
		public string Name => string.IsNullOrEmpty(Device.Name) ? "<Unknown>" : Device.Name;

		/// <summary>
		/// Gets the Bluetooth Low Energy MAC address of this device (empty 
		/// if it is running in iOS).
		/// </summary>
		public string BleMac => Regex.Replace(Device.Id.ToString().Split('-')[4], MAC_REGEX, MAC_REPLACE).ToUpper();

		/// <summary>
		/// Gets whether the device is connected or not.
		/// </summary>
		public bool IsConnected => Device.State == DeviceState.Connected;

		/// <summary>
		/// Gets and sets the RSSI of this device.
		/// </summary>
		public int Rssi
		{
			get { return rssi; }
			set
			{
				if (value < 0 && value != RssiUtils.WORST_RSSI)
					rssi = value;

				LastUpdated = DateTimeOffset.Now.ToUnixTimeMilliseconds();
				IsActive = true;

				RaisePropertyChangedEvent(nameof(Rssi));
				RaisePropertyChangedEvent(nameof(RssiImage));
			}
		}

		/// <summary>
		/// Gets the image corresponding with the current RSSI value.
		/// </summary>
		public string RssiImage => RssiUtils.GetRssiImage(rssi);

		/// <summary>
		/// Gets the time when this device was last updated.
		/// </summary>
		public long LastUpdated { get; private set; }

		/// <summary>
		/// Gets and sets whether this device is active or not.
		/// </summary>
		public bool IsActive
		{
			get { return isActive; }
			set
			{
				isActive = value;
				RaisePropertyChangedEvent(nameof(IsActive));
			}
		}

		/// <summary>
		/// Gets the XBee device object associated to this device.
		/// </summary>
		public XBeeBLEDevice XBeeDevice { get; private set; }

		/// <summary>
		/// Indicates whether the device being reset or not.
		/// </summary>
		public bool ResettingDevice { get; set; }

		/// <summary>
		/// Indicates whether the BLE interface of the device is being disabled or not.
		/// </summary>
		public bool DisablingBLE { get; set; }

		/// <summary>
		/// Sets the password of the bluetooth device.
		/// </summary>
		public string Password
		{
			set
			{
				XBeeDevice.SetBluetoothPassword(value);
			}
		}

		/// <summary>
		/// Indicates whether the device is connecting or not.
		/// </summary>
		public bool Connecting { get; set; } = false;

		// Events.
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Class constructor. Instantiates a new <c>BleDevice</c> object with
		/// the given native device.
		/// </summary>
		/// <param name="device">The native Bluetooth device.</param>
		public BleDevice(IDevice device)
		{
			Device = device ?? throw new ArgumentNullException(nameof(device), "Device cannot be null");
			Rssi = device.Rssi;

			XBeeDevice = new XBeeBLEDevice(device, null);
		}

		/// <summary>
		/// Connects the XBee device.
		/// </summary>
		public void Connect()
		{
			XBeeDevice.Connect();
		}

		/// <summary>
		/// Disconnects the XBee device.
		/// </summary>
		public void Disconnect()
		{
			XBeeDevice.Disconnect();
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			BleDevice dev = (BleDevice)obj;
			return Id == dev.Id;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		/// <summary>
		/// Triggers a property changed event for the property with the given name.
		/// </summary>
		/// <param name="propertyName">The name of the property to trigger a changed event for.</param>
		protected void RaisePropertyChangedEvent(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChangedEventArgs e = new(propertyName);
				PropertyChanged(this, e);
			}
		}
	}
}
