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

using BLENetworkConfigurationSample.Utils;
using BLENetworkConfigurationSample.Utils.Validators;
using DigiIoT.Maui.Devices.ConnectCore;
using DigiIoT.Maui.Events;
using DigiIoT.Maui.Exceptions;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using XBeeLibrary.Core.Exceptions;

namespace BLENetworkConfigurationSample.Models
{
	public class BleDevice : INotifyPropertyChanged
	{
		// Constants.
		public static readonly string MAC_REGEX = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
		public static readonly string MAC_REPLACE = "$1:$2:$3:$4:$5:$6";

		private const string PREFIX_PWD = "pwd_";

		public static readonly int INACTIVITY_TIME = 10000;

		private static readonly int TIMEOUT_DEFAULT = 5;

		// Variables.
		private int rssi = RssiUtils.WORST_RSSI;

		private bool isActive = true;

		private readonly List<Interface> interfaces = new();

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
		/// Gets the ConnectCore device object associated to this device.
		/// </summary>
		public ConnectCoreBLEDevice ConnectCoreDevice { get; private set; }

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
				ConnectCoreDevice.SetBluetoothPassword(value);
			}
		}

		/// <summary>
		/// Indicates whether the device is connecting or not.
		/// </summary>
		public bool Connecting { get; set; } = false;

		/// <summary>
		/// List of interfaces of the device.
		/// </summary>
		public List<Interface> Interfaces => interfaces;

		// Events.
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Class constructor. Instantiates a new <c>ConnectCoreDevice</c> object with
		/// the given native device.
		/// </summary>
		/// <param name="device">The native Bluetooth device.</param>
		public BleDevice(IDevice device)
		{
			Device = device ?? throw new ArgumentNullException(nameof(device), "Device cannot be null");
			Rssi = device.Rssi;

			ConnectCoreDevice = new ConnectCoreBLEDevice(device, null);

			InitializeInterfaces();
		}

		/// <summary>
		/// Initializes the device configurable interfaces.
		/// </summary>
		private void InitializeInterfaces()
		{
			GenerateNetworkInterface("Ethernet");
			GenerateNetworkInterface("WiFi");
		}

		/// <summary>
		/// Generates a new network interface.
		/// </summary>
		/// <param name="interfaceName">Name of the interface.</param>
		private void GenerateNetworkInterface(string interfaceName)
		{
			List<AbstractSetting> settings = new()
			{
				new BooleanSetting("Enabled", "False"),
				new TextSetting("MAC", "00:00:00:00:00:00", new MACValidator()),
				new ComboSetting("Type", "static", new Dictionary<string, string>() { { "static", "Static" }, { "dhcp", "DHCP" } }),
				new TextSetting("IP", "0.0.0.0", new IPValidator()),
				new TextSetting("Netmask", "0.0.0.0", new IPValidator()),
				new TextSetting("Gateway", "0.0.0.0", new IPValidator()),
				new TextSetting("DNS", "0.0.0.0", new IPValidator())
			};
			interfaces.Add(new Interface(interfaceName, settings, this));
		}

		/// <summary>
		/// Connects the ConnectCore device.
		/// </summary>
		public void Connect()
		{
			ConnectCoreDevice.Connect();
		}

		/// <summary>
		/// Disconnects the ConnectCore device.
		/// </summary>
		public void Disconnect()
		{
			ConnectCoreDevice.Disconnect();
		}

		/// <summary>
		/// Sends the given data to the device.
		/// </summary>
		/// <param name="data">The data to send.</param>
		/// <returns>The execution answer.</returns>
		/// <exception cref="CommunicationException">If there is any error sending the data.</exception>
		public async Task<string> SendData(string data)
		{
			return await SendData(data, TIMEOUT_DEFAULT);
		}

		/// <summary>
		/// Sends the given data to the device.
		/// </summary>
		/// <param name="data">The data to send.</param>
		/// <returns>The execution answer.</returns>
		/// <exception cref="CommunicationException">If there is any error sending the data.</exception>
		public async Task<string> SendData(byte[] data)
		{
			return await SendData(data, TIMEOUT_DEFAULT);
		}

		/// <summary>
		/// Sends the given data to the device.
		/// </summary>
		/// <param name="data">The data to send.</param>
		/// <param name="timeout">The execution timeout.</param>
		/// <returns>The execution answer.</returns>
		/// <exception cref="CommunicationException">If there is any error sending the data.</exception>
		public async Task<string> SendData(string data, int timeout)
		{
			// Convert data to byte array.
			byte[] byteData = Encoding.ASCII.GetBytes(data);
			// Send the data.
			return await SendData(byteData, timeout);
		}

		/// <summary>
		/// Sends the given data to the device.
		/// </summary>
		/// <param name="data">The data to send.</param>
		/// <param name="timeout">The execution timeout.</param>
		/// <returns>The execution answer.</returns>
		/// <exception cref="CommunicationException">If there is any error sending the data.</exception>
		public async Task<string> SendData(byte[] data, int timeout)
		{
			string answer = null;
			ManualResetEvent waitHandle = new(false);

			if (!ConnectCoreDevice.IsConnected)
			{
				throw new CommunicationException("Device is not connected");
			}

			void DataReceived(object sender, DataReceivedEventArgs e)
			{
				// Extract JSON string
				answer = Encoding.ASCII.GetString(e.Data, 0, e.Data.Length);
				Console.WriteLine("Received data: " + answer);
				// Notify wait handle.
				waitHandle.Set();
			}

			// Register data received callback.
			ConnectCoreDevice.DataReceived += DataReceived;
			// Send the data.
			await Task.Run(() =>
			{
				try
				{
					ConnectCoreDevice.SendData(data);
					// Wait for answer.
					waitHandle.WaitOne(timeout * 1000);
				}
				catch (DigiIoTException ex2)
				{
					ConnectCoreDevice.DataReceived -= DataReceived;
					throw new CommunicationException("Error sending data: " + ex2.Message);
				}
			});
			// Remove data received callback.
			ConnectCoreDevice.DataReceived -= DataReceived;

			return answer ?? throw new CommunicationException("Timeout waiting for execution answer");
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
