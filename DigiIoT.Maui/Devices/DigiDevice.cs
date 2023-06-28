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

using DigiIoT.Maui.Events;
using DigiIoT.Maui.Exceptions;
using Plugin.BLE.Abstractions.Contracts;
using XBeeLibrary.Core;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;

namespace DigiIoT.Maui.Devices
{
	/// <summary>
	/// This class represents a generic Digi Bluetooth Low Energy (BLE) device.
	/// </summary>
	public class DigiDevice : AbstractXBeeDevice
	{
		// Properties.
		/// <summary>
		/// Indicates whether the device is a remote device or not.
		/// </summary>
		public override bool IsRemote => false;

		/// <summary>
		/// The status of the Bluetooth connection. It indicates if the device 
		/// is connected or not.
		/// </summary>
		/// <seealso cref="Connect"/>
		/// <seealso cref="Disconnect"/>
		public bool IsConnected => IsOpen;

		// Events.
		/// <summary>
		/// Represents the method that will handle the Bluetooth data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="BLEDataReceivedEventArgs"/>
		public event EventHandler<BLEDataReceivedEventArgs> BLEDataReceived;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="DigiDevice"/> object with the
		/// given parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Connect"/> method,
		/// either through this constructor or the <see cref="SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="device">Bluetooth device to connect to.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <seealso cref="IDevice"/>
		public DigiDevice(IDevice device, string password)
			: base(DeviceUtils.CreateConnectionInterface(device))
		{
			bluetoothPassword = password;
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="DigiDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Connect"/> method,
		/// either through this constructor or the <see cref="SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="deviceAddress">The address or GUID of the Bluetooth device. It must follow the
		/// format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c> for the address or
		/// <c>01234567-0123-0123-0123-0123456789AB</c> for the GUID.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <exception cref="ArgumentException">If <paramref name="deviceAddress"/> does not follow
		/// the format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c> or
		/// <c>01234567-0123-0123-0123-0123456789AB</c>.</exception>
		public DigiDevice(string deviceAddress, string password)
			: base(DeviceUtils.CreateConnectionInterface(deviceAddress))
		{
			bluetoothPassword = password;
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="DigiDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Connect"/> method,
		/// either through this constructor or the <see cref="SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="deviceGuid">The Bluetooth device GUID.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <seealso cref="Guid"/>
		public DigiDevice(Guid deviceGuid, string password)
			: base(DeviceUtils.CreateConnectionInterface(deviceGuid))
		{
			bluetoothPassword = password;
		}

		/// <summary>
        /// Resets the device.
        /// </summary>
		public override void Reset()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Opens the Bluetooth connection with the device.
		/// </summary>
		/// <exception cref="DigiIoTException">If there is any problem connecting with the device.</exception>
		/// <seealso cref="IsConnected"/>
		/// <seealso cref="Disconnect"/>
		public void Connect()
		{
			try
			{
				Open();
				UserDataRelayReceived += Device_UserDataRelayReceived;
			}
			catch (XBeeException e)
			{
				throw new DigiIoTException(e);
			}
		}

		/// <summary>
		/// Closes the Bluetooth connection with the device.
		/// </summary>
		/// <exception cref="DigiIoTException">If there is any problem disconnecting the device.</exception>
		/// <seealso cref="IsConnected"/>
		/// <seealso cref="Connect"/>
		public void Disconnect()
		{
			try
			{
				Close();
				UserDataRelayReceived -= Device_UserDataRelayReceived;
			}
			catch (XBeeException e)
			{
				throw new DigiIoTException(e);
			}
		}

		/// <summary>
		/// Sets the Bluetooth password of the Digi device in order to connect to it.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Connect"/> method.
		/// </remarks>
		/// <param name="password">The Bluetooth password of the device.</param>
		public new void SetBluetoothPassword(string password)
		{
			base.SetBluetoothPassword(password);
		}

		/// <summary>
		/// Returns the string representation of this device.
		/// </summary>
		/// <returns>The string representation of this device.</returns>
		public override string ToString()
		{
			return base.ToString();
		}

		/// <summary>
		/// Sends the given data to the device.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="DigiIoTException">If there is any error sending the data.</exception>
		public void SendData(byte[] data)
		{
			try
			{
				SendSerialData(data);
			}
			catch (XBeeException e)
			{
				throw new DigiIoTException(e);
			}
		}

		/// <summary>
		/// Reads a Bluetooth data packet received by the device during the configured receive 
		/// timeout.
		/// </summary>
		/// <remarks>This method blocks until new data is received or the configured receive 
		/// timeout expires.</remarks>
		/// <returns>A byte array containing the data. <c>null</c> if the device did not 
		/// receive new data during the configured receive timeout.</returns>
		/// <exception cref="DigiIoTException">If the device is not connected.</exception>
		/// <seealso cref="ReadData(int)"/>
		public new byte[] ReadData()
		{
			UserDataRelayMessage message = null;
			try
			{
				message = ReadUserDataRelay();
			}
			catch (XBeeException e)
			{
				throw new DigiIoTException(e);
			}
			if (message == null)
				return null;
			return message.Data;
		}

		/// <summary>
		/// Reads a Bluetooth data packet received by the device during the provided timeout.
		/// </summary>
		/// <remarks>This method blocks until new data is received or the given timeout 
		/// expires.</remarks>
		/// <param name="timeout">The time to wait for new User Data Relay in 
		/// milliseconds.</param>
		/// <returns>A byte array containing the data. <c>null</c> if the device did not
		/// receive new data during <paramref name="timeout"/> milliseconds.</returns>
		/// <exception cref="DigiIoTException">If the device is not connected.</exception>
		/// <seealso cref="ReadData()"/>
		public new byte[] ReadData(int timeout)
		{
			UserDataRelayMessage message = null;
			try
			{
				message = ReadUserDataRelay(timeout);
			}
			catch (XBeeException e)
			{
				throw new DigiIoTException(e);
			}
			if (message == null)
				return null;
			return message.Data;
		}

		/// <summary>
		/// Handles the user data relay received event notifying about the data received to all the
		/// <code>BLEDataReceived</code> registered callbacks.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Device_UserDataRelayReceived(object sender, UserDataRelayReceivedEventArgs e)
		{
			if (BLEDataReceived != null)
			{
				try
				{
					lock (BLEDataReceived)
					{
						var handler = BLEDataReceived;
						if (handler != null)
						{
							var args = new BLEDataReceivedEventArgs(e.UserDataRelayMessage.Data);
							handler.GetInvocationList().AsParallel().ForAll((action) =>
							{
								action.DynamicInvoke(this, args);
							});
						}
					}
				}
				catch (Exception ex)
				{
					logger.Error(ex.Message, ex);
				}
			}
		}
	}
}
