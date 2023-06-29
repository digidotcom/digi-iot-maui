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

using DigiIoT.Maui.Services.Bluetooth;
using DigiIoT.Maui.Services.GPS;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace DigiIoT.Maui.Models
{
	/// <summary>
	/// Class used to manage Bluetooth events and permissions.
	/// </summary>
    public class DigiBLEManager
	{
		// Constants.
		public const string ERROR_BLUETOOTH_DISABLED_TITLE = "Bluetooth disabled";
		public const string ERROR_BLUETOOTH_DISABLED = "Bluetooth is required to communicate with BLE devices, please turn it on.";
		public const string ERROR_BLUETOOTH_PERMISSION_TITLE = "Permission not granted";
		public const string ERROR_BLUETOOTH_PERMISSION = "'Nearby devices' permissions were not granted. Please, enable application permissions from 'Settings'.";
		public const string ERROR_LOCATION_DISABLED_TITLE = "Location disabled";
		public const string ERROR_LOCATION_DISABLED = "Location is required to work with BLE devices, please turn it on.";
		public const string ERROR_LOCATION_PERMISSION_TITLE = "Permission not granted";
		public const string ERROR_LOCATION_PERMISSION = "'Location' permissions were not granted. Please, enable application permissions from 'Settings'.";
		public const string ERROR_CHECK_PERMISSION_TITLE = "Error checking permissions";
		public const string ERROR_CHECK_PERMISSION = "There was an error checking the location permission: {0}";

		private const int MIN_ANDROID_VERSION_FOR_LOCATION = 6;

		// Properties.
		/// <summary>
		/// Indicates whether Bluetooth is scanning or not.
		/// </summary>
		public bool IsScanning { get; private set; }

		// Delegates.
		public delegate void DeviceAdvertisedHandler(IDevice advertisedDevice);
		public delegate void BluetoothStateChangedHandler(BluetoothState state);
		public delegate void ConnectionLostHandler();
		public delegate void ScanStartedHandler();
		public delegate void ScanStoppedHandler();

		// Events.
		public event DeviceAdvertisedHandler DeviceAdvertised;
		public event BluetoothStateChangedHandler BluetoothStateChanged;
		public event ConnectionLostHandler ConnectionLost;
		public event ScanStartedHandler ScanStarted;
		public event ScanStoppedHandler ScanStopped;

		// Variables.
		private readonly IAdapter adapter;

		// Methods.
		/// <summary>
		/// Class constructior. Instantiates a new <c>BluetoothManager</c> object.
		/// </summary>
		public DigiBLEManager()
		{
			// Init variables.
			adapter = CrossBluetoothLE.Current.Adapter;
			IsScanning = false;
			// Listen for changes in the bluetooth adapter.
			CrossBluetoothLE.Current.StateChanged += (o, e) =>
			{
                BluetoothStateChanged?.Invoke(e.NewState);
            };
			// Listen for device advertisements.
			adapter.DeviceAdvertised += (sender, e) =>
			{
                DeviceAdvertised?.Invoke(e.Device);
            };
			// Listen for connection lost events.
			adapter.DeviceConnectionLost += (sender, e) =>
			{
                ConnectionLost?.Invoke();
            };
		}

		/// <summary>
		/// Starts the scan process to look for BLE devices.
		/// </summary>
		public async void StartScanning()
		{
			StopScanning();
			IsScanning = true;
            ScanStarted?.Invoke();
            await adapter.StartScanningForDevicesAsync();
		}

		/// <summary>
		/// Stops the BLE scanning process.
		/// </summary>
		public void StopScanning()
		{
			IsScanning = false;
            ScanStopped?.Invoke();
            Task task = Task.Run(async () =>
			{
				try
				{
                    await adapter.StopScanningForDevicesAsync();
                }
				catch (Exception)
                { 
					// Sometimes, the stop scanning process generates a System.AggregateException indicating that
					// the CancellationTokenSource has been disposed.
				}
				
			});
            // Block and wait for task to complete
            task.Wait();
		}

		/// <summary>
		/// Returns whether the Bluetooth adapter is enabled or not.
		/// </summary>
		/// <returns><c>true</c> if Bluetooth is enabled, <c>false</c> otherwise.</returns>
		public static async Task<bool> IsEnabled()
		{
			BluetoothState state = await GetState();
			return state != BluetoothState.Off;
		}

		/// <summary>
		/// Returns the current Bluetooth state.
		/// </summary>
		/// <returns>The Bluetooth state</returns>
		public static Task<BluetoothState> GetState()
		{
			IBluetoothLE ble = CrossBluetoothLE.Current;
			TaskCompletionSource<BluetoothState> tcs = new();

			// In some cases the Bluetooth state is unknown the first time it is read.
			if (ble.State == BluetoothState.Unknown)
			{
				// Listen for changes in the Bluetooth adapter.
				void handler(object o, BluetoothStateChangedArgs e)
				{
					CrossBluetoothLE.Current.StateChanged -= handler;
					tcs.SetResult(e.NewState);
				}
				CrossBluetoothLE.Current.StateChanged += handler;
			}
			else
			{
				tcs.SetResult(ble.State);
			}

			return tcs.Task;
		}

        /// <summary>
        /// Returns whether the location is enabled or not.
        /// </summary>
        /// <returns><c>true</c> if location is enabled, <c>false</c> otherwise.</returns>
        public static bool IsLocationEnabled()
        {
            if (DeviceInfo.Version.Major < MIN_ANDROID_VERSION_FOR_LOCATION)
            {
                return true;
            }
            else
            {
                GPSStatusService service = new GPSStatusService();
                try
                {
                    return service.IsGPSEnabled();
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Requests location permisions.
        /// </summary>
        /// <returns><c>true</c> if permission was granted, <c>false</c> otherwise.</returns>
        public static async Task<bool> RequestLocationPermission()
		{
			PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
			if (permissionStatus != PermissionStatus.Granted)
			{
				permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
				return permissionStatus == PermissionStatus.Granted;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Requests Bluetooth permisions.
		/// </summary>
		/// <returns><c>true</c> if permission was granted, <c>false</c> otherwise.</returns>
		public static async Task<bool> RequestBluetoothPermission()
		{
			BLEPermissionsService service = new BLEPermissionsService();
            try
			{
				return await service.RequestBLEPermissions();
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
