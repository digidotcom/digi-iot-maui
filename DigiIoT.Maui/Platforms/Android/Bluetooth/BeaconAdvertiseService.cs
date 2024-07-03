/*
 * Copyright 2024, Digi International Inc.
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

using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;

namespace DigiIoT.Maui.Services.Bluetooth
{
	/// <summary>
	/// Partial class that includes the custom Android implementation of the Beacon
	/// advertise service.
	/// </summary>
	public partial class BeaconAdvertiseService
	{
		// Constants.
		private static readonly byte[] BEACON_ID_IBEACON = [0x02, 0x15];

		private const int START_ADV_TIMEOUT = 1000;  // 1 second

		// Variables.
		private BluetoothLeAdvertiser _bluetoothLeAdvertiser;
		private CustomAdvertiseCallback _advertiseCallback;
		private BluetoothAdapter _bluetoothAdapter;

		private byte[] _advertisementData;

		private ManualResetEventSlim _callbackEvent = new ManualResetEventSlim(false);

		public BeaconAdvertiseService()
		{
			var bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Context.BluetoothService);
			_bluetoothAdapter = bluetoothManager.Adapter;
		}

		/// <summary>
		/// Starts advertising a custom beacon with the provided parameters.
		/// </summary>
		/// <param name="identifier">Identifier or local name of the beacon.</param>
		/// <param name="companyID">Company ID used in the manufacturer-specific data.</param>
		/// <param name="manufacturerData">Manufacturer-specific data to advertise.</param>
		/// <returns><code>true</code> if the process started successfully, <code>false</code>
		/// otherwise.</returns>
		public partial bool StartAdvertisingCustomBeacon(string identifier, byte[] companyID, byte[] manufacturerData)
		{
			return StartAdvertisingInternal(identifier, companyID, manufacturerData);
		}

		/// <summary>
		/// Starts advertising an iBeacon with the provided parameters.
		/// </summary>
		/// <param name="identifier">Identifier or local name of the beacon.</param>
		/// <param name="uuid">UUID of the iBeacon.</param>
		/// <param name="major">Major value of the iBeacon.</param>
		/// <param name="minor">Minor value of the iBeacon.</param>
		/// <returns><code>true</code> if the process started successfully, <code>false</code>
		/// otherwise.</returns>
		public partial bool StartAdvertisingIBeacon(string identifier, string uuid, ushort major, ushort minor)
		{
			// Generate the iBeacon payload.
			var manufacturerData = BEACON_ID_IBEACON.
				Concat(HexStringToByteArray(uuid)).
				Concat(BitConverter.GetBytes(major).Reverse()).
				Concat(BitConverter.GetBytes(minor).Reverse()).
				Concat(TX_POWER_DEFAULT).ToArray();

			return StartAdvertisingInternal(identifier, COMPANY_ID_APPLE, manufacturerData);
		}

		/// <summary>
		/// Starts advertising with the proviced parameters.
		/// </summary>
		/// <param name="identifier">Identifier or local name of the beacon.</param>
		/// <param name="companyID">Company ID used in the manufacturer-specific data.</param>
		/// <param name="payload">Payload of the beacon.</param>
		/// <returns><code>true</code> if the process started successfully, <code>false</code>
		/// otherwise.</returns>
		private bool StartAdvertisingInternal(string identifier, byte[] companyID, byte[] payload)
		{
			if (_bluetoothAdapter == null)
			{
				// Handle case where Bluetooth is not supported or not enabled
				return false;
			}

			_bluetoothLeAdvertiser = _bluetoothAdapter.BluetoothLeAdvertiser;
			_advertiseCallback = new CustomAdvertiseCallback(_callbackEvent);

			// Update the advertisement name.
			_bluetoothAdapter.SetName(identifier);

			// Set the advertisement options.
			var advertiseSettingsBuilder = new AdvertiseSettings.Builder()
				.SetAdvertiseMode(AdvertiseMode.LowLatency)
				.SetTxPowerLevel(AdvertiseTx.PowerHigh);
			if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake)
				advertiseSettingsBuilder = advertiseSettingsBuilder.SetDiscoverable(true);
			var advertiseSettings = advertiseSettingsBuilder.Build();

			// Generate the advertisement data.
			var beaconLenght =
				(payload != null ? payload.Length + companyID.Length + 2 : 0)  // 2 extra bytes for mfg data length and mfg data tag.
				+ (identifier != null ? identifier.Length + 2 : 0)             // 2 extra bytes for name length and name tag.
				+ 3;                                                           // 3 extra bytes for flags length, flags tag and flags value.
			var advertiseDataBuilder = new AdvertiseData.Builder()
				.SetIncludeDeviceName(identifier != null && beaconLenght <= 31)  // 31 is the maximum allowed beacon length.
				.SetIncludeTxPowerLevel(false);
			if (payload != null)
				advertiseDataBuilder = advertiseDataBuilder.AddManufacturerData(BitConverter.ToUInt16(companyID, 0), payload);
			var advertiseData = advertiseDataBuilder.Build();

			// Start advertising.
			_bluetoothLeAdvertiser.StartAdvertising(advertiseSettings, advertiseData, _advertiseCallback);

			// Wait for the callback to signal completion.
			_callbackEvent.Wait(START_ADV_TIMEOUT);

			return _advertiseCallback.GetCallbackResult();
		}

		/// <summary>
		/// Stops advertising.
		/// </summary>
		public partial void StopAdvertising()
		{
			_bluetoothLeAdvertiser?.StopAdvertising(_advertiseCallback);
		}

		/// <summary>
		/// Converts a hexadecimal string to a byte array.
		/// </summary>
		/// <param name="hex">The hexadecimal string to convert.</param>
		/// <returns>A byte array representing the hexadecimal string.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the input hex
		/// string is null.</exception>
		/// <exception cref="System.FormatException">Thrown when the input hex string
		/// contains non-hexadecimal characters.</exception>
		public static byte[] HexStringToByteArray(string hex)
		{
			if (hex == null)
				throw new ArgumentNullException(nameof(hex), "Hex string cannot be null.");

			hex = hex.Replace("-", "");
			if (hex.Length % 2 != 0)
				hex = "0" + hex;

			byte[] bytes = new byte[hex.Length / 2];
			for (int i = 0; i < hex.Length; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}
	}

	/// <summary>
	/// Callback required for the advertising process. Notifies about the
	/// status of the start advertising process.
	/// </summary>
	public class CustomAdvertiseCallback : AdvertiseCallback
	{
		// Variables.
		private bool _callbackResult = false;

		private ManualResetEventSlim _callbackEvent;

		/// <summary>
		/// Class constructor. Instantiates a new <code>CustomAdvertiseCallback</code>
		/// with the provided parameters.
		/// </summary>
		/// <param name="callbackEvent">The callback event to notify when a result is received.</param>
		public CustomAdvertiseCallback(ManualResetEventSlim callbackEvent)
		{
			_callbackEvent = callbackEvent;
			_callbackEvent.Reset();
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override void OnStartFailure(AdvertiseFailure errorCode)
		{
			base.OnStartFailure(errorCode);

			// Set the callback result.
			_callbackResult = false;
			_callbackEvent.Set();
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
		{
			base.OnStartSuccess(settingsInEffect);

			// Set the callback result.
			_callbackResult = true;
			_callbackEvent.Set();
		}

		/// <summary>
		/// Returns the callback result.
		/// </summary>
		/// <returns><code>true</code> if the result was success, <code>false</code>
		/// otherwise.</returns>
		public bool GetCallbackResult()
		{
			return _callbackResult;
		}
	}
}