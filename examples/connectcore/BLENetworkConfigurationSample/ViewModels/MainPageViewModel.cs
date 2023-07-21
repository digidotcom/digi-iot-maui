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
using BLENetworkConfigurationSample.Pages;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using DigiIoT.Maui.Exceptions;
using DigiIoT.Maui.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using System.Windows.Input;
using XBeeLibrary.Core.Exceptions;

namespace BLENetworkConfigurationSample.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
		// Constants.
		private const string ERROR_BLUETOOTH_TITLE = "Error checking Bluetooth";
		private const string ERROR_DISCONNECTED_TITLE = "Device disconnected";
		private const string ERROR_DISCONNECTED = "Connection lost with the device.";
		private const string ERROR_RECONNECT_TITLE = "Reconnect error";
		private const string ERROR_RECONNECT = "There was an error trying to reconnect with the device > {0}";
		private const string ERROR_CONNECT_TITLE = "Connect error";
		private const string ERROR_CONNECT = "Could not connect to the XBee device > {0}";
		private const string ERROR_ALREADY_CONNECTED = "The selected XBee device is already connected";

		private const string TASK_CONNECT = "Connecting to '{0}'...";
		private const string TASK_RECONNECT = "Trying to reconnect with the device...";

		private const string TEXT_SCAN = "Scan";
		private const string TEXT_STOP = "Stop";

		private const string STATUS_SCANNING = "Scanning";
		private const string STATUS_STOPPED = "Scan stopped";

		private const int SCAN_INTERVAL = 10000;

		private const int OPEN_RETRIES = 3;

		// Variables.
		private bool activePage = true;

		private bool isEmpty;
		private bool isScanning;
		private bool isRefreshing;
		private bool continuousScanInitialized = false;
		private bool scanEnabled = true;
		private bool initializingScan = false;

		private string scanStatus;
		private string scanButtonText;

		private ObservableCollection<BleDevice> devices;
		private BleDevice selectedDevice;

		private readonly DigiBLEManager bluetoothManager;
		private readonly DigiBLEManager.ScanStartedHandler scanStartedHandler;
		private readonly DigiBLEManager.ScanStoppedHandler scanStoppedHandler;
		private readonly DigiBLEManager.ConnectionLostHandler connectionLostHandler;
		private readonly DigiBLEManager.DeviceAdvertisedHandler deviceAdvertisedHandler;
		private readonly DigiBLEManager.BluetoothStateChangedHandler bluetoothStateChangedHandler;

		private string password;

		// Properties.
		/// <summary>
		/// Indicates whether the page is active or not.
		/// </summary>
		public bool ActivePage
		{
			get { return activePage; }
			set
			{
				activePage = value;
				RaisePropertyChangedEvent(nameof(ActivePage));
			}
		}

		/// <summary>
		/// Indicates whether the list of devices is empty or not.
		/// </summary>
		public bool IsEmpty
		{
			get { return isEmpty; }
			set
			{
				isEmpty = value;
				RaisePropertyChangedEvent(nameof(IsEmpty));
			}
		}

		/// <summary>
		/// Indicates whether the scan process is running or not.
		/// </summary>
		public bool IsScanning
		{
			get { return isScanning; }
			set
			{
				isScanning = value;
				RaisePropertyChangedEvent(nameof(IsScanning));
			}
		}

		/// <summary>
		/// Indicates whether the list of devices is being refreshed or not.
		/// </summary>
		public bool IsRefreshing
		{
			get { return isRefreshing; }
			set
			{
				isRefreshing = value;
				RaisePropertyChangedEvent(nameof(IsRefreshing));
			}
		}

		/// <summary>
		/// Scan status label.
		/// </summary>
		public string ScanStatus
		{
			get { return scanStatus; }
			set
			{
				scanStatus = value;
				RaisePropertyChangedEvent(nameof(ScanStatus));
			}
		}

		/// <summary>
		/// Scan status button text.
		/// </summary>
		public string ScanButtonText
		{
			get { return scanButtonText; }
			set
			{
				scanButtonText = value;
				RaisePropertyChangedEvent(nameof(ScanButtonText));
			}
		}

		/// <summary>
		/// List of filtered devices found over BLE.
		/// </summary>
		public ObservableCollection<BleDevice> Devices
		{
			get { return devices; }
			set
			{
				devices = value;
				RaisePropertyChangedEvent(nameof(Devices));
			}
		}

		/// <summary>
		/// The selected device to establish a BLE connection with.
		/// </summary>
		public BleDevice SelectedDevice
		{
			get { return selectedDevice; }
			set
			{
				selectedDevice = value;
				RaisePropertyChangedEvent(nameof(SelectedDevice));
				ConnectToDevice();
			}
		}

		/// <summary>
		/// Enable status of the scan process.
		/// </summary>
		public bool ScanEnabled
		{
			get { return scanEnabled; }
			set
			{
				scanEnabled = value;
				RaisePropertyChangedEvent(nameof(ScanEnabled));
			}
		}

		// Commands.
		/// <summary>
		/// Command used to toggle the scan process.
		/// </summary>
		public ICommand ToggleScan { get; private set; }

		/// <summary>
		/// Command used to refresh the list of discovered devices.
		/// </summary>
		public ICommand RefreshList { get; private set; }

		// Delegate functions.
		/// <summary>
		/// Handles what happens when the Bluetooth scan starts.
		/// </summary>
		private void ScanStarted()
		{
			ScanStatus = STATUS_SCANNING;
			ScanButtonText = TEXT_STOP;
			IsScanning = true;
		}

		/// <summary>
		/// Handles what happens when the Bluetooth scan stops.
		/// </summary>
		private void ScanStopped()
		{
			ScanStatus = STATUS_STOPPED;
			ScanButtonText = TEXT_SCAN;
			IsScanning = false;
		}

		/// <summary>
		/// Handles what happens when the connection with the device is lost.
		/// </summary>
		internal void ConnectionLost()
		{
			if (selectedDevice == null || CrossBluetoothLE.Current.State != BluetoothState.On || selectedDevice.Connecting)
				return;

			// Close the connection with the device.
			selectedDevice.Disconnect();

			// If the device is being reset try to reconnect it. Otherwise display 
			// an error and navigate to the devices list (root) page.
			if (selectedDevice.ResettingDevice)
			{
				ReconnectToDevice();
			}
			else
			{
				// If BLE interface is about to be disabled, hide the writing dialog.
				if (selectedDevice.DisablingBLE)
				{
					HideLoadingDialog();
					selectedDevice.DisablingBLE = false;
				}

				// Return to main page and prompt the disconnection error.
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.Navigation.PopToRootAsync();
					DisplayAlert(ERROR_DISCONNECTED_TITLE, ERROR_DISCONNECTED);
				});
			}
		}

		/// <summary>
		/// Handles what happens when a Bluetooth device is advertised.
		/// </summary>
		/// <param name="advertIDevice">Advertised device.</param>
		public void DeviceAdvertised(IDevice advertIDevice)
		{
			BleDevice advertisedDevice = new(advertIDevice);
			// If the device is not discovered, add it to the list. Otherwise update its RSSI value.
			if (!devices.Contains(advertisedDevice))
				AddDeviceToList(advertisedDevice);
			else
				devices[devices.IndexOf(advertisedDevice)].Rssi = advertisedDevice.Rssi;
		}

		/// <summary>
		/// Handles what happens when the Bluetooth state changes.
		/// </summary>
		/// <param name="state">The new bluetooth state.</param>
		private async void BluetoothStateChanged(BluetoothState state)
		{
			if (state == BluetoothState.Off)
			{
				ScanEnabled = false;

				// Stop scanning.
				if (bluetoothManager.IsScanning)
					bluetoothManager.StopScanning();

				// Clear the devices list.
				ClearDevicesList();

				// Disconnect the device.
				if (selectedDevice != null)
				{
					try
					{
						selectedDevice.Disconnect();
					}
					catch (Exception) { }
				}

				await Application.Current.MainPage.Navigation.PopToRootAsync();

				// If there is a loading dialog, close it.
				HideLoadingDialog();
			}
			else if (state == BluetoothState.On)
			{
				// Start scanning.
				if (ViewModelBase.GetCurrentPage() is MainPage)
				{
					await Task.Run(() =>
					{
						StartScan();
					});
				}
			}
		}

		/// <summary>
		/// Class constructor. Instantiates a new <c>MainPageViewModel</c> object.
		/// </summary>
		public MainPageViewModel()
		{
			// Init variables.
			bluetoothManager = new DigiBLEManager();
			scanStartedHandler = new DigiBLEManager.ScanStartedHandler(ScanStarted);
			scanStoppedHandler = new DigiBLEManager.ScanStoppedHandler(ScanStopped);
			connectionLostHandler = new DigiBLEManager.ConnectionLostHandler(ConnectionLost);
			deviceAdvertisedHandler = new DigiBLEManager.DeviceAdvertisedHandler(DeviceAdvertised);
			bluetoothStateChangedHandler = new DigiBLEManager.BluetoothStateChangedHandler(BluetoothStateChanged);
			
			devices = new ObservableCollection<BleDevice>();

			// Define behavior of Scan button.
			ToggleScan = new Command(ToggleScanControl);
			RefreshList = new Command(RefreshListControl);
		}

		/// <summary>
		/// Subscribes the Bluetooth listeners to be notified about the different events
		/// happening in the interface.
		/// </summary>
		public void SubscribeBluetoothListeners()
		{
			bluetoothManager.ConnectionLost += connectionLostHandler;
			bluetoothManager.ScanStarted += scanStartedHandler;
			bluetoothManager.ScanStopped += scanStoppedHandler;
			bluetoothManager.DeviceAdvertised += deviceAdvertisedHandler;
			bluetoothManager.BluetoothStateChanged += bluetoothStateChangedHandler;
		}

		/// <summary>
		/// Disables the page controls.
		/// </summary>
		public void DisableControls()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				ScanEnabled = false;
				ScanButtonText = TEXT_SCAN;
			});
		}

		/// <summary>
		/// Starts a continous scan of BLE devices.
		/// </summary>
		public void StartContinuousScan()
		{
			// Clear the list of BLE devices.
			ClearDevicesList();

			bluetoothManager.StartScanning();

			if (continuousScanInitialized)
				return;
			continuousScanInitialized = true;

			// Timer task to re-start the scan process.
			IDispatcherTimer scanTimer = Application.Current.Dispatcher.CreateTimer();
			scanTimer.Interval = TimeSpan.FromMilliseconds(SCAN_INTERVAL);
			scanTimer.Tick += (s, e) => {
				if (bluetoothManager.IsScanning && ActivePage)
				{
					bluetoothManager.StartScanning();
				}
				else
				{
					scanTimer.Stop();
				}
			};
			scanTimer.Start();

			// Timer task to check inactive devices.
			IDispatcherTimer checkTimer = Application.Current.Dispatcher.CreateTimer();
			checkTimer.Interval = TimeSpan.FromMilliseconds(BleDevice.INACTIVITY_TIME);
			checkTimer.Tick += (s, e) => {
				if (bluetoothManager.IsScanning && ActivePage)
				{
					foreach (BleDevice device in Devices)
					{
						device.IsActive = DateTimeOffset.Now.ToUnixTimeMilliseconds() - device.LastUpdated < BleDevice.INACTIVITY_TIME;
					}
				}
				else
				{
					scanTimer.Stop();
				}
			};
			checkTimer.Start();
		}

		/// <summary>
		/// Starts the Bluetooth scan process.
		/// </summary>
		public bool StartScan()
		{
			if (initializingScan)
				return false;
			initializingScan = true;
			ScanEnabled = true;

			// Start scanning BLE devices.
			StartContinuousScan();
			initializingScan = false;
			return true;
		}

		/// <summary>
		/// Stops the Bluetooth scan process.
		/// </summary>
		public void StopScan()
		{
			bluetoothManager.StopScanning();
		}

		/// <summary>
		/// Connects to the selected XBee BLE device.
		/// </summary>
		private async void ConnectToDevice()
		{
			if (selectedDevice == null)
				return;

			bluetoothManager.StopScanning();

			if (selectedDevice.IsConnected)
				DisplayAlert(ERROR_CONNECT_TITLE, ERROR_ALREADY_CONNECTED);
			else
			{
				selectedDevice.Connecting = true;
				await Task.Run(async () =>
				{
					ShowLoadingDialog(string.Format(TASK_CONNECT, selectedDevice.Name));

					try
					{
						// Open the connection with the device. Try to connect up to 3 times.
						var retries = OPEN_RETRIES;
						while (!selectedDevice.ConnectCoreDevice.IsConnected && retries > 0)
						{
							retries--;
							try
							{
								selectedDevice.Connect();
							}
							catch (Exception e)
							{
								// If the number of retries is 0 or there was a problem with 
								// the Bluetooth authentication, raise the exception.
								if (e is BluetoothAuthenticationException || retries == 0)
									throw;
							}
							await Task.Delay(1000);
						}

						MainThread.BeginInvokeOnMainThread(async () =>
						{
							// Load device information page.
							await Application.Current.MainPage.Navigation.PushAsync(new DevicePage(selectedDevice));
						});
					}
					catch (BluetoothAuthenticationException)
					{
						HideLoadingDialog();

						selectedDevice.Disconnect();

						password = await MainPageViewModel.AskForPassword(true);

						// If the user cancelled the dialog, return.
						if (password == null)
							return;

						selectedDevice.Password = password;

						MainThread.BeginInvokeOnMainThread(() =>
						{
							ConnectToDevice();
						});
					}
					catch (Exception ex)
					{
						DisplayAlert(ERROR_CONNECT_TITLE, string.Format(ERROR_CONNECT, ex.Message));

						// Disconnect the device. It was established and a problem occurred when reading settings.
						selectedDevice.Disconnect();
						SelectedDevice = null;
					}
					finally
					{
						HideLoadingDialog();
						if (selectedDevice != null)
							selectedDevice.Connecting = false;
					}
				});
			}
		}

		/// <summary>
		/// Reconnects to the selected XBee BLE device.
		/// </summary>
		private async void ReconnectToDevice()
		{
			await Task.Run(() =>
			{
				HideLoadingDialog();
				ShowLoadingDialog(TASK_RECONNECT);
				try
				{
					// Connect the device.
					selectedDevice.Connect();
				}
				catch (Exception ex)
				{
					// If there is an error connecting, display the message and navigate to the 
					// devices list (root) page.
					MainThread.BeginInvokeOnMainThread(async () =>
					{
						DisplayAlert(ERROR_RECONNECT_TITLE, string.Format(ERROR_RECONNECT, ex.Message));
						await Application.Current.MainPage.Navigation.PopToRootAsync();
					});
				}
				finally
				{
					HideLoadingDialog();
					// Reset the 'Reset' flag.
					selectedDevice.ResettingDevice = false;
				}
			});
		}

		/// <summary>
		/// Toggles the scan process.
		/// </summary>
		public void ToggleScanControl()
		{
			if (bluetoothManager.IsScanning)
			{
				bluetoothManager.StopScanning();
				IsRefreshing = false;
			}
			else
			{
				ClearDevicesList();
				bluetoothManager.StartScanning();
			}
		}

		/// <summary>
		/// Refreshes the list of the discovered devices.
		/// </summary>
		public void RefreshListControl()
		{
			// As the scan is done every 10 seconds while scanning, the refresh process
			// only requires for the list to be cleared, then, as devices will keep
			// coming it will create the illusion of the list being refreshed.
			IsRefreshing = true;
			ClearDevicesList();

			// If the scan is stopped, start it.
			if (!bluetoothManager.IsScanning)
				bluetoothManager.StartScanning();
		}

		/// <summary>
		/// Clears the list of discovered devices.
		/// </summary>
		public void ClearDevicesList()
		{
			devices.Clear();
			IsEmpty = true;
		}

		/// <summary>
		/// Adds a new device to the list of discovered devices.
		/// </summary>
		/// <param name="deviceToAdd"></param>
		public void AddDeviceToList(BleDevice deviceToAdd)
		{
			devices.Add(deviceToAdd);

			// Update the refresh status.
			if (IsRefreshing)
				IsRefreshing = false;

			IsEmpty = devices.Count == 0;
		}

		/// <summary>
		/// Prepares the connection with the given device.
		/// </summary>
		/// <param name="device">BLE device to connect to.</param>
		public async void PrepareConnection(BleDevice device)
		{
			password = await AskForPassword(false);

			// If the user cancelled the dialog, return.
			if (password == null)
				return;

			device.Password = password;

			ActivePage = false;

			// Save the selected device and connect to it.
			SelectedDevice = device;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override bool DisconnectBeforeNavigatingBack()
		{
			return false;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override void DisconnectDevice()
		{
			// Nothing to do here.
		}

		/// <summary>
		/// Checks Bluetooth availability, required services and permissions.
		/// </summary>
		/// <returns><c>true</c> if Bluetooth as well as required permissions are present, <c>false</c> otherwise.</returns>
		public static bool CheckBluetooth()
		{
			try
			{
				DigiBLEManager.ValidateBluetoothStatus();
			}
			catch (DigiIoTException ex)
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					DisplayAlert(ERROR_BLUETOOTH_TITLE, ex.Message);
				});
				return false;
			}
			return true;
		}

		/// <summary>
		/// Shows a popup asking the user for the password and returns it.
		/// </summary>
		/// <param name="authFailed">Whether the first authentication attempt
		/// failed or not.</param>
		/// <returns>The password entered by the user or <c>null</c> if the
		/// dialog was cancelled.</returns>
		public static Task<string> AskForPassword(bool authFailed)
		{
			var tcs = new TaskCompletionSource<string>();

			PasswordPage passwordPage = new(authFailed);
			passwordPage.Closed += (object sender, PopupClosedEventArgs e) =>
			{
				string pwd = passwordPage.Password;
				tcs.SetResult(pwd);
			};

			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await Shell.Current.ShowPopupAsync(passwordPage);
			});

			return tcs.Task;
		}
	}
}
