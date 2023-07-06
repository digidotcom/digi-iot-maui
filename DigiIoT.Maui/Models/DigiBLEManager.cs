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
		private const int MIN_ANDROID_VERSION_FOR_LOCATION = 6;

		// Properties.
		/// <summary>
		/// Indicates whether Bluetooth is scanning or not.
		/// </summary>
		public bool IsScanning { get; private set; }

		// Delegates.
		/// <summary>
		/// Notifies that a Bluetooth device has been advertised.
		/// </summary>
		/// <param name="advertisedDevice">The advertised device.</param>
		public delegate void DeviceAdvertisedHandler(IDevice advertisedDevice);

        /// <summary>
        /// Notifies that the Bluetooth interface state has changed.
        /// </summary>
        /// <param name="state">The new Bluetooth state</param>
        public delegate void BluetoothStateChangedHandler(BluetoothState state);

        /// <summary>
        /// Notifies that the Bluetooth connection was lost.
        /// </summary>
        public delegate void ConnectionLostHandler();

        /// <summary>
        /// Notifies that the Bluetooth scan process has started.
        /// </summary>
        public delegate void ScanStartedHandler();

        /// <summary>
        /// Notifies that the Bluetooth scan process has stopped.
        /// </summary>
        public delegate void ScanStoppedHandler();

		// Events.
		/// <summary>
		/// Event called when a Bluetooth device has been advertised.
		/// </summary>
		public event DeviceAdvertisedHandler DeviceAdvertised;

        /// <summary>
        /// Event called when the Bluetooth interface state changed.
        /// </summary>
        public event BluetoothStateChangedHandler BluetoothStateChanged;

        /// <summary>
        /// Event called when the Bluetooth connection was lost.
        /// </summary>
        public event ConnectionLostHandler ConnectionLost;

        /// <summary>
        /// Event called when the Bluetooth scan process has started.
        /// </summary>
        public event ScanStartedHandler ScanStarted;

        /// <summary>
        /// Event called when the Bluetooth scan process has stopped.
        /// </summary>
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
                try
                {
                    return GPSStatusService.IsGPSEnabled();
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
