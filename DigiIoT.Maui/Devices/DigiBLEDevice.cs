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

using DigiIoT.Maui.Devices.XBee;
using DigiIoT.Maui.Events;
using Plugin.BLE.Abstractions.Contracts;

namespace DigiIoT.Maui.Devices
{
    /// <summary>
    /// This class represents a generic Digi device with Bluetooth Low Energy (BLE) connectivity.
    /// </summary>
    public class DigiBLEDevice : IDigiBLEDevice
	{
		
		// Properties.
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public bool IsConnected => _xBeeDevice.IsConnected;

        // Events.
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived
		{
			add
			{
				_xBeeDevice.DataReceived += value;
			}
			remove
			{
				_xBeeDevice.DataReceived -= value;
			}
		}

		// Variables.
		private readonly XBeeBLEDevice _xBeeDevice;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="DigiBLEDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Connect"/> method,
		/// either through this constructor or the <see cref="SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="device">Bluetooth device to connect to.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <seealso cref="IDevice"/>
		public DigiBLEDevice(IDevice device, string password)
		{
			_xBeeDevice = new XBeeBLEDevice(device, password);
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="DigiBLEDevice"/> object with the given 
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
		public DigiBLEDevice(string deviceAddress, string password)
		{
			_xBeeDevice = new XBeeBLEDevice(deviceAddress, password);
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="DigiBLEDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Connect"/> method,
		/// either through this constructor or the <see cref="SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="deviceGuid">The Bluetooth device GUID.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <seealso cref="Guid"/>
		public DigiBLEDevice(Guid deviceGuid, string password)
		{
			_xBeeDevice = new XBeeBLEDevice(deviceGuid, password);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public void Connect()
		{
			_xBeeDevice?.Connect();
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public void Disconnect()
		{
			_xBeeDevice?.Disconnect();
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public byte[] ReadData()
		{
			return _xBeeDevice.ReadData();
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public byte[] ReadData(int timeout)
		{
			return _xBeeDevice.ReadData(timeout);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public void SendData(byte[] data)
		{
			_xBeeDevice.SendData(data);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public void SetBluetoothPassword(string password)
		{
			_xBeeDevice.SetBluetoothPassword(password);
		}

		/// <summary>
		/// Returns the string representation of this device.
		/// </summary>
		/// <returns>The string representation of this device.</returns>
		public override string ToString()
		{
			return _xBeeDevice.ToString();
		}
	}
}
