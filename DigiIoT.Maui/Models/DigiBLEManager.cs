/*
 * Copyright 2023,2024, Digi International Inc.
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

using DigiIoT.Maui.Exceptions;
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
		private const string ERROR_BLUETOOTH_DISABLED = "Bluetooth interface is not enabled.";
		private const string ERROR_GPS_DISABLED = "GPS is not enabled and it is required by the application.";
		private const string ERROR_LOCATION_PERMISSION_NOT_GRANTED = "Location permission was not granted.";
		private const string ERROR_BLUETOOTH_PERMISSION_NOT_GRANTED = "Bluetooth permissions were not granted.";

		private const int MIN_ANDROID_VERSION_FOR_LOCATION = 6;
		private const int MAX_ANDROID_VERSION_FOR_LOCATION = 11;

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
		/// Notifies that the device was disconnected.
		/// </summary>
		public delegate void DeviceDisconnectedHandler();

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
		/// Event called when the device is disconnected.
		/// </summary>
		public event DeviceDisconnectedHandler DeviceDisconnected;

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
			// Listen for device disconnected events.
			adapter.DeviceDisconnected += (sender, e) =>
			{
				DeviceDisconnected?.Invoke();
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
		public static async Task<bool> IsBluetoothEnabled()
		{
			BluetoothState state = await GetBluetoothState();
			return state != BluetoothState.Off;
		}

		/// <summary>
		/// Returns the current Bluetooth state.
		/// </summary>
		/// <returns>The Bluetooth state</returns>
		public static Task<BluetoothState> GetBluetoothState()
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
		/// Returns whether the GPS is enabled or not.
		/// </summary>
		/// <returns><c>true</c> if the GPS is enabled, <c>false</c> otherwise.</returns>
		public static bool IsGPSEnabled()
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

		/// <summary>
		/// Requests location permisions.
		/// </summary>
		/// <returns><c>true</c> if permission was granted, <c>false</c> otherwise.</returns>
		public static Task<bool> RequestLocationPermission()
		{
			var tcs = new TaskCompletionSource<bool>();
			var semaphore = new SemaphoreSlim(0, 1);
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
					if (permissionStatus != PermissionStatus.Granted)
					{
						permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
					}
					tcs.TrySetResult(permissionStatus == PermissionStatus.Granted);
				}
				catch (Exception ex)
				{
					tcs.TrySetResult(false);
					tcs.TrySetException(ex);
				}
				finally
				{
					semaphore.Release();
				}
			});
			return WaitForResult(tcs, semaphore);
		}

		/// <summary>
		/// Requests Bluetooth permisions.
		/// </summary>
		/// <returns><c>true</c> if permission was granted, <c>false</c> otherwise.</returns>
		public static Task<bool> RequestBluetoothPermission()
		{
			BLEPermissionsService service = new();
			var tcs = new TaskCompletionSource<bool>();
			var semaphore = new SemaphoreSlim(0, 1);
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					var result = await service.RequestBLEPermissions();
					tcs.TrySetResult(result);
				}
				catch (Exception ex)
				{
					tcs.TrySetResult(false);
					tcs.TrySetException(ex);
				}
				finally
				{
					semaphore.Release();
				}
			});
			return WaitForResult(tcs, semaphore);
		}

		/// <summary>
		/// Validates that Bluetooth is operative in the device. This means that necessary interfaces
		/// and permissions are enabled and granted. The method requests for permissions to the user if necessary.
		/// </summary>
		/// <exception cref="DigiIoTException">If any required interface is not enabled or necessary permission
		/// was not granted.</exception>
		public static void ValidateBluetoothStatus()
		{
			ValidateBluetoothStatus(false);
		}

		/// <summary>
		/// Validates that Bluetooth is operative in the device. This means that necessary interfaces
		/// and permissions are enabled and granted. The method requests for permissions to the user if necessary.
		/// </summary>
		/// <param name="forceLocationPermissions"><c>true</c> if the application
		/// using the BLE manager requires location permissions. <c>false</c>
		/// otherwise.</param>
		/// <exception cref="DigiIoTException">If any required interface is not enabled or necessary permission
		/// was not granted.</exception>
		public static void ValidateBluetoothStatus(bool forceLocationPermissions)
		{
			var bluetoothEnabledTask = IsBluetoothEnabled();
			bluetoothEnabledTask.Wait();
			if (!bluetoothEnabledTask.Result)
			{
				throw new DigiIoTException(ERROR_BLUETOOTH_DISABLED);
			}
			if (DeviceInfo.Current.Platform == DevicePlatform.Android)
			{
				if (MIN_ANDROID_VERSION_FOR_LOCATION <= DeviceInfo.Version.Major && (forceLocationPermissions || DeviceInfo.Version.Major <= MAX_ANDROID_VERSION_FOR_LOCATION))
				{
					if (!IsGPSEnabled())
					{
						throw new DigiIoTException(ERROR_GPS_DISABLED);
					}
					var locationPermissionTask = RequestLocationPermission();
					locationPermissionTask.Wait();
					if (!locationPermissionTask.Result)
					{
						throw new DigiIoTException(ERROR_LOCATION_PERMISSION_NOT_GRANTED);
					}
				}
				if (DeviceInfo.Version.Major > MAX_ANDROID_VERSION_FOR_LOCATION)
				{
					var bluetoothPermissionTask = RequestBluetoothPermission();
					bluetoothPermissionTask.Wait();
					if (!bluetoothPermissionTask.Result)
					{
						throw new DigiIoTException(ERROR_BLUETOOTH_PERMISSION_NOT_GRANTED);
					}
				}
			}
			else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
			{
				if (forceLocationPermissions)
				{
					if (!IsGPSEnabled())
					{
						throw new DigiIoTException(ERROR_GPS_DISABLED);
					}
					var locationPermissionTask = RequestLocationPermission();
					locationPermissionTask.Wait();
					if (!locationPermissionTask.Result)
					{
						throw new DigiIoTException(ERROR_LOCATION_PERMISSION_NOT_GRANTED);
					}
				}
				var bluetoothPermissionTask = RequestBluetoothPermission();
				bluetoothPermissionTask.Wait();
				if (!bluetoothPermissionTask.Result)
				{
					throw new DigiIoTException(ERROR_BLUETOOTH_PERMISSION_NOT_GRANTED);
				}
			}
		}

		/// <summary>
		/// Asynchronously waits for the provided semaphore to be available and then
		/// waits for the given TaskCompletionSource result.
		/// </summary>
		/// <param name="tcs">The TaskCompletionSource to await the result from.</param>
		/// <param name="semaphore">The SemaphoreSlim to wait for before awaiting the
		/// TaskCompletionSource.</param>
		/// <returns>A task that represents the asynchronous operation. The task result
		/// contains the boolean value from the TaskCompletionSource.</returns>
		private static async Task<bool> WaitForResult(TaskCompletionSource<bool> tcs, SemaphoreSlim semaphore)
		{
			await semaphore.WaitAsync();
			return await tcs.Task;
		}
	}
}
