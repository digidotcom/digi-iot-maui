﻿/*
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
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Diagnostics;
using System.Text.RegularExpressions;
using XBeeLibrary.Core.Connection;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Utils;

namespace DigiIoT.Maui.Connection.Bluetooth
{
	/// <summary>
	/// Class that handles the connection with a Digi device through Bluetooth Low Energy.
	/// </summary>
	/// <seealso cref="IConnectionInterface"/>
	public class BluetoothInterface : IConnectionInterface
	{
		// Constants.
		private static readonly int CONNECTION_TIMEOUT = 20000;
		private static readonly int DISCONNECTION_TIMEOUT = 5000;
		private static readonly int SUBSCRIBE_TIMEOUT = 3000;
		private static readonly int WRITE_TIMEOUT = 3000;
		private static readonly int WRITE_TIMEOUT_LONG = 6000;

		private static readonly int LENGTH_COUNTER = 16;

		private static readonly int REQUESTED_MTU = 512; // Maximum ATT layer MTU size.

		private static readonly string ERROR_INVALID_MAC_GUID = "Invalid MAC address or GUID, it has to follow the format 00112233AABB or " +
			"00:11:22:33:AA:BB for the MAC address or 01234567-0123-0123-0123-0123456789AB for the GUID";
		private static readonly string ERROR_CONNECTION = "Could not connect to the Digi BLE device";
		private static readonly string ERROR_CONNECTION_CLOSED = "Device is not connected";
		private static readonly string ERROR_CONNECTION_TIMEOUT = "Timeout while opening connection";
		private static readonly string ERROR_DISCONNECTION = "Could not disconnect the Digi BLE device";
		private static readonly string ERROR_WRITE = "Timeout writing in the TX Characteristic";
		private static readonly string ERROR_GET_SERVICE = "Could not get the communication service";
		private static readonly string ERROR_GET_CHARS = "Could not get the communication characteristics";
		private static readonly string ERROR_SUBSCRIBE_RX_CHAR = "Could not subscribe to RX characteristic";

		private static readonly string SERVICE_GUID = "53DA53B9-0447-425A-B9EA-9837505EB59A";
		private static readonly string TX_CHAR_GUID = "7DDDCA00-3E05-4651-9254-44074792C590";
		private static readonly string RX_CHAR_GUID = "F9279EE9-2CD0-410C-81CC-ADF11E4E5AEA";

		private static readonly string MAC_REGEX = "^([0-9A-Fa-f]{12})|([0-9A-Fa-f]{2}[:]){5}([0-9A-Fa-f]{2})$";
		private static readonly string GUID_REGEX = "^[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}$";
		private static readonly string MAC_GUID = "00000000-0000-0000-0000-{0}";

		private static readonly string INTERFACE_NAME = "Bluetooth devuce";

		// Variables.
		private readonly IAdapter adapter;
		private IDevice device;

		private readonly Guid deviceGuid;

		private ICharacteristic txCharacteristic;
		private ICharacteristic rxCharacteristic;

		private bool encrypt = false;
		private bool subscribed = false;

		private CounterModeCryptoTransform encryptor;
		private CounterModeCryptoTransform decryptor;

		static readonly SemaphoreSlim deviceSemaphore = new(initialCount: 1);

		private int mtu;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="BluetoothInterface"/> object.
		/// </summary>
		private BluetoothInterface()
		{
			adapter = CrossBluetoothLE.Current.Adapter;

			Stream = new DataStream();
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="BluetoothInterface"/> object with the given 
		/// device.
		/// </summary>
		/// <param name="device">Bluetooth device.</param>
		/// <seealso cref="IDevice"/>
		public BluetoothInterface(IDevice device) : this()
		{
			this.device = device;
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="BluetoothInterface"/> object with the given 
		/// Bluetooth device GUID.
		/// </summary>
		/// <param name="deviceAddress">The address or GUID of the Bluetooth device. It must follow the
		/// format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c> for the address or
		/// <c>01234567-0123-0123-0123-0123456789AB</c> for the GUID.</param>
		/// <exception cref="ArgumentException">If <paramref name="deviceAddress"/> does not follow
		/// the format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c> or
		/// <c>01234567-0123-0123-0123-0123456789AB</c>.</exception>
		public BluetoothInterface(string deviceAddress) : this()
		{
			if (!Regex.IsMatch(deviceAddress, MAC_REGEX) && !Regex.IsMatch(deviceAddress, GUID_REGEX))
				throw new ArgumentException(ERROR_INVALID_MAC_GUID);

			if (Regex.IsMatch(deviceAddress, MAC_REGEX))
				deviceGuid = Guid.Parse(string.Format(MAC_GUID, deviceAddress.Replace(":", "")));
			else
				deviceGuid = new Guid(deviceAddress);
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="BluetoothInterface"/> object with the given 
		/// Bluetooth device GUID.
		/// </summary>
		/// <param name="deviceGuid">The Bluetooth device GUID.</param>
		/// <seealso cref="Guid"/>
		public BluetoothInterface(Guid deviceGuid) : this()
		{
			this.deviceGuid = deviceGuid;
		}

		// Properties.
		/// <summary>
		/// Returns whether the connection interface is open or not.
		/// </summary>
		/// <seealso cref="Close"/>
		/// <seealso cref="Open"/>
		public bool IsOpen { get; private set; } = false;

		/// <summary>
		/// Returns the bluetooth connection interface stream to read and write data.
		/// </summary>
		/// <seealso cref="DataStream"/>
		public DataStream Stream { get; }

		/// <summary>
		/// Opens the connection interface associated with this Bluetooth device.
		/// </summary>
		/// <exception cref="DigiIoTException">If there is any problem opening the connection
		/// with this bluetooth device.</exception>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Close"/>
		public void Open()
		{
			Debug.WriteLine("----- Open");

			// Do nothing if the device is already open.
			if (IsOpen)
			{
				return;
			}

			// Initialize variables.
			string connectExceptionMessage = null;

			// Create a task to connect the device.
			Task task = Task.Run(async () =>
			{
				try
				{
					// Force connection transport to be BLE. This fixes an issue with some Android devices
					// throwing the error '133' while connecting with a BLE device and requesting the GATT
					// server services and characteristics.
					ConnectParameters parameters = new(forceBleTransport: true);

					// Abort the connect operation if the given timeout expires.
					CancellationTokenSource cancelToken = new(CONNECTION_TIMEOUT);

					// Connect to the device.
					if (device == null)
					{
						device = await adapter.ConnectToKnownDeviceAsync(deviceGuid, parameters, cancelToken.Token);
					}
					else
					{
						await adapter.ConnectToDeviceAsync(device, parameters, cancelToken.Token);
					}

					// Check if token expired.
					if (cancelToken.IsCancellationRequested)
					{
						throw new DigiIoTException(ERROR_CONNECTION_TIMEOUT);
					}
					cancelToken.Dispose();

					// Check if device is connected.
					if (device != null && device.State == DeviceState.Connected)
					{
						IsOpen = true;
					}
					else
					{
						throw new DigiIoTException(ERROR_CONNECTION);
					}

					// Request MTU and connection interval.
					mtu = await device.RequestMtuAsync(REQUESTED_MTU);
					bool highInterval = device.UpdateConnectionInterval(ConnectionInterval.High);
					Debug.WriteLine("----- MTU: " + mtu);
					Debug.WriteLine("----- Connection interval high (11-15ms): " + highInterval);

					// Get the TX and RX characteristics.
					cancelToken = new CancellationTokenSource(WRITE_TIMEOUT);

					// Some smartphones require a brief idle period before executing the 'GetServiceAsync' task
					// on the 'IDevice'; otherwise, the method hangs. Empirical tests suggest that 500ms is
					// sufficient, but we have increased it to 1s to ensure reliability.
					await Task.Delay(1000);

					IService service = await device.GetServiceAsync(Guid.Parse(SERVICE_GUID), cancellationToken: cancelToken.Token);
					if (service == null || cancelToken.IsCancellationRequested)
					{
						throw new DigiIoTException(ERROR_GET_SERVICE);
					}
					cancelToken.Dispose();
					txCharacteristic = await service.GetCharacteristicAsync(Guid.Parse(TX_CHAR_GUID));
					rxCharacteristic = await service.GetCharacteristicAsync(Guid.Parse(RX_CHAR_GUID));
					if (txCharacteristic == null || rxCharacteristic == null)
					{
						throw new DigiIoTException(ERROR_GET_CHARS);
					}

					// Subscribe to the RX characteristic.
					if (rxCharacteristic.CanUpdate)
					{
						rxCharacteristic.ValueUpdated += DataReceived;
						cancelToken = new CancellationTokenSource(SUBSCRIBE_TIMEOUT);
						await rxCharacteristic.StartUpdatesAsync(cancellationToken: cancelToken.Token);
						if (cancelToken.IsCancellationRequested)
						{
							throw new DigiIoTException(ERROR_SUBSCRIBE_RX_CHAR);
						}
						cancelToken.Dispose();
						subscribed = true;
					}

					// Initialize the connection without encryption for the SRP handshake protocol.
					encrypt = false;
				}
				catch (Exception e)
				{
					connectExceptionMessage = e.Message == null ? ERROR_CONNECTION : ERROR_CONNECTION + " > " + e.Message;
				}
			});

			// Wait until the task finishes.
			bool completed = task.Wait(CONNECTION_TIMEOUT);

			// Check if task completed.
			if (!completed)
			{
				// If the close process fails, do not override the main cause
				// of the connection problem.
				try
				{
					Close();
				}
				catch (DigiIoTException) { }
				throw new DigiIoTException(ERROR_CONNECTION_TIMEOUT);
			}

			// If the task finished with excepction, throw it.
			if (connectExceptionMessage != null)
			{
				// If the close process fails, do not override the main cause
				// of the connection problem.
				try
				{
					Close();
				}
				catch (DigiIoTException) { }
				throw new DigiIoTException(connectExceptionMessage);
			}

			// Check again if the device is connected. We've seen that sometimes the 
			// Rx subscribe process could make the device to disconnect.
			if (device != null && device.State != DeviceState.Connected)
			{
				// If the close process fails, do not override the main cause
				// of the connection problem.
				try
				{
					Close();
				}
				catch (DigiIoTException) { }
				throw new DigiIoTException(ERROR_CONNECTION);
			}
		}

		/// <summary>
		/// Closes the connection interface associated with this bluetooth device.
		/// </summary>
		/// <exception cref="DigiIoTException">If there is any error closing the connection.</exception>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Open"/>
		public void Close()
		{
			Debug.WriteLine("----- Close");

			// Do nothing if the device is not open.
			if (!IsOpen)
			{
				return;
			}

			// Wait for other device operations.
			deviceSemaphore.Wait(WRITE_TIMEOUT);

			// Create a task to disconnect the device.
			Task task = Task.Run(async () =>
			{
				// Unsubscribe from the RX characteristic.
				if (subscribed)
				{
					CancellationTokenSource cancelToken = new(SUBSCRIBE_TIMEOUT);
					try
					{
						await MainThread.InvokeOnMainThreadAsync(async () =>
						{
							await rxCharacteristic.StopUpdatesAsync(cancellationToken: cancelToken.Token);
						});
						if (cancelToken.IsCancellationRequested)
						{
							Debug.WriteLine("----- BLE interface - Timeout unsubscribing RX characteristic");
						}
					}
					catch (Exception e)
					{
						Debug.WriteLine("----- BLE interface - Error unsubscribing RX characteristic: " + e.Message);
					}
					finally
					{
						rxCharacteristic.ValueUpdated -= DataReceived;
						subscribed = false;
						cancelToken.Dispose();
					}
				}

				// Disconnect the device.
				try
				{
					if (device != null && device.State == DeviceState.Connected)
					{
						await adapter.DisconnectDeviceAsync(device);
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine("----- BLE interface - Error disconnecting the device: " + e.Message);
				}

				IsOpen = false;
			});

			// Wait until the task finishes.
			bool completed = task.Wait(DISCONNECTION_TIMEOUT);

			// Check if disconnect process finished.
			try
			{
				if (!completed || (device != null && device.State != DeviceState.Disconnected && device.State != DeviceState.Limited))
				{
					// Set interface connection as closed (although there was a problem closing it...)
					IsOpen = false;
					throw new DigiIoTException(ERROR_DISCONNECTION);
				}
			}
			finally
			{
				deviceSemaphore.Release();
			}
		}

		/// <summary>
		/// Reads data from the bluetooth connection interface and stores it in the provided byte 
		/// array returning the number of read bytes.
		/// </summary>
		/// <param name="data">The byte array to store the read data.</param>
		/// <returns>The number of bytes read.</returns>
		/// <seealso cref="ReadData(byte[], int, int)"/>
		public int ReadData(byte[] data)
		{
			return ReadData(data, 0, data.Length);
		}

		/// <summary>
		/// Reads the given number of bytes at the given offset from the bluetooth connection interface 
		/// and stores it in the provided byte array returning the number of read bytes.
		/// </summary>
		/// <param name="data">The byte array to store the read data.</param>
		/// <param name="offset">The start offset in data array at which the data is written.</param>
		/// <param name="length">Maximum number of bytes to read.</param>
		/// <returns>The number of bytes read.</returns>
		/// <seealso cref="ReadData(byte[])"/>
		public int ReadData(byte[] data, int offset, int length)
		{
			Debug.WriteLine("----- ReadData " + data.Length);

			int readBytes = 0;
			if (Stream != null)
				readBytes = Stream.Read(data, offset, length);
			return readBytes;
		}

		/// <summary>
		/// Writes the given data in the bluetooth connection interface.
		/// </summary>
		/// <param name="data">The data to be written in the connection interface.</param>
		/// <exception cref="DigiIoTException">If there is any error writing data.</exception>
		/// <seealso cref="WriteData(byte[], int, int)"/>
		public void WriteData(byte[] data)
		{
			try
			{
				WriteData(data, 0, data.Length);
			}
			catch (XBeeException e)
			{
				throw new DigiIoTException(e);
			}
		}

		/// <summary>
		/// Writes the given data in the bluetooth connection interface.
		/// </summary>
		/// <param name="data">The data to be written in the connection interface.</param>
		/// <param name="offset">The start offset in the data to write.</param>
		/// <param name="length">The number of bytes to write.</param>
		/// <exception cref="DigiIoTException">If there is any error writing data.</exception>
		/// <seealso cref="WriteData(byte[])"/>
		public void WriteData(byte[] data, int offset, int length)
		{
			// Wait for other device operations.
			deviceSemaphore.Wait(WRITE_TIMEOUT);
			if (!IsOpen)
			{
				deviceSemaphore.Release();
				throw new DigiIoTException(ERROR_CONNECTION_CLOSED);
			}

			byte[] dataToWrite = new byte[length];
			Array.Copy(data, offset, dataToWrite, 0, length);

			// Abort the write operation if the write timeout expires.
			CancellationTokenSource cancelToken = new(WRITE_TIMEOUT);
			int success = -1;
			Task writeTask = Task.Run(async () =>
			{
				// According to BLE.Plugin API documentation, every write operation
				// must be executed in the main thread.
				await MainThread.InvokeOnMainThreadAsync(async () =>
				{
					try
					{
						// Split the data in slices with a max length of the current MTU minus 3.
						foreach (byte[] slice in SliceData(dataToWrite))
						{
							// Prepare data to write.
							Debug.WriteLine("----- WriteData " + HexUtils.ByteArrayToHexString(slice));
							byte[] sliceBuffer = new byte[slice.Length];
							Array.Copy(slice, 0, sliceBuffer, 0, slice.Length);
							byte[] sliceToWrite = encrypt ? encryptor.TransformFinalBlock(sliceBuffer, 0, sliceBuffer.Length) : slice;

							// Write the slice in the TX characteristic.
							success = await txCharacteristic.WriteAsync(sliceToWrite, cancellationToken: cancelToken.Token);
							if (success != 0)
								throw new DigiIoTException(ERROR_WRITE);
						}
					}
					catch (Exception)
					{
						encryptor.DecrementCounter();
					}
					finally
					{
						cancelToken.Dispose();
					}
				});
			});
			// Wait for write operation to finish.
			writeTask.Wait(WRITE_TIMEOUT_LONG);

			// Free semaphore.
			deviceSemaphore.Release();

			// Check for error.
			if (success != 0)
			{
				throw new DigiIoTException(ERROR_WRITE);
			}
		}

		/// <summary>
		/// Returns the connection type of this bluetooth interface.
		/// </summary>
		/// <returns>The connection type of this bluetooth interface.</returns>
		/// <seealso cref="ConnectionType"/>
		public ConnectionType GetConnectionType()
		{
			return ConnectionType.BLUETOOTH;
		}

		/// <summary>
		/// Returns the Attribute MTU negotiated with the BLE Gatt server.
		/// </summary>
		/// <returns>The Attribute MTU negotiated with the BLE Gatt server.</returns>
		public int getAttMTU()
		{
			return mtu;
		}

		/// <summary>
		/// Method executed when new data is received in the RX characteristic.
		/// </summary>
		/// <param name="sender">Characteristic Updated Event sender.</param>
		/// <param name="args">Characteristic Updated Event arguments.</param>
		/// <seealso cref="CharacteristicUpdatedEventArgs"/>
		private void DataReceived(object sender, CharacteristicUpdatedEventArgs args)
		{
			byte[] value = args.Characteristic.Value;

			// If the communication is encrypted, decrypt the received data.
			if (encrypt)
				value = decryptor.TransformFinalBlock(value, 0, value.Length);

			Debug.WriteLine("----- RX char " + HexUtils.ByteArrayToHexString(value));

			Stream.Write(value, 0, value.Length);

			// Notify that data has been received.
			lock (this)
			{
				Monitor.Pulse(this);
			}
		}

		/// <summary>
		/// Sets the encryption keys and starts to encrypt the communication
		/// with the module.
		/// </summary>
		/// <param name="key">Session key.</param>
		/// <param name="txNonce">TX nonce used as prefix of the counter block.
		/// </param>
		/// <param name="rxNonce">RX nonce used as prefix of the counter block.
		/// </param>
		public void SetEncryptionKeys(byte[] key, byte[] txNonce, byte[] rxNonce)
		{
			Aes128CounterMode aesEncryption = new(BluetoothInterface.GetCounter(txNonce, 1));
			encryptor = (CounterModeCryptoTransform)aesEncryption.CreateEncryptor(key, null);

			Aes128CounterMode aesDecryption = new(BluetoothInterface.GetCounter(rxNonce, 1));
			decryptor = (CounterModeCryptoTransform)aesDecryption.CreateEncryptor(key, null);

			encrypt = true;
		}

		/// <summary>
		/// Generates and returns the encryption counter with the given nonce
		/// and count value.
		/// </summary>
		/// <param name="nonce">Nonce used as prefix of the counter block.</param>
		/// <param name="count">Count value.</param>
		/// <returns>The encryption counter.</returns>
		private static byte[] GetCounter(byte[] nonce, int count)
		{
			byte[] counter = new byte[LENGTH_COUNTER];
			Array.Copy(nonce, 0, counter, 0, nonce.Length);
			byte[] countBytes = ByteUtils.IntToByteArray(count);
			Array.Copy(countBytes, 0, counter, nonce.Length, countBytes.Length);
			return counter;
		}

		/// <summary>
		/// Returns the string representation of the Bluetooth interface.
		/// </summary>
		/// <returns>The string representation of the Bluetooth interface.</returns>
		public override string ToString()
		{
			return INTERFACE_NAME;
		}


		/// <summary>
		/// Returns a list with the data slices of the given byte array. The
		/// maximum length of each slice is the negotiated MTU minus 3 bytes.
		/// 
		/// According to the Bluetooth core specification, the attribute protocol
		/// needs 3 bytes for the opcode (write command) and the attribute handle
		/// (which attribute to write to). So, the actual data length is always
		/// ATT_MTU minus 3.
		/// </summary>
		/// <param name="data">Data to get the slices from.</param>
		/// <returns>A list with the data slices.</returns>
		private List<byte[]> SliceData(byte[] data)
		{
			List<byte[]> slices = new();

			if (data.Length <= (mtu - 3))
			{
				slices.Add(data);
			}
			else
			{
				int i = 0;
				while (i < data.Length)
				{
					int remainingLength = data.Length - i;
					int bufferLength = remainingLength < (mtu - 3) ? remainingLength : (mtu - 3);
					byte[] buffer = new byte[bufferLength];
					Array.Copy(data, i, buffer, 0, bufferLength);
					slices.Add(buffer);
					i += bufferLength;
				}
			}

			return slices;
		}
	}
}
