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

namespace DigiIoT.Maui.Devices
{
	/// <summary>
	/// Interface used to define the minimum and required methods that a Digi device class should
	/// include to establish Bluetooth Low Energy (BLE) communications.
	/// </summary>
	internal interface IDigiBLEDevice
	{
		// Properties.
		/// <summary>
		/// The status of the Bluetooth connection. It indicates if the device 
		/// is connected or not.
		/// </summary>
		/// <seealso cref="Connect"/>
		/// <seealso cref="Disconnect"/>
		public bool IsConnected
		{
			get;
		}

		// Events.
		/// <summary>
		/// Represents the method that will handle the Bluetooth data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="DataReceivedEventArgs"/>
		public event EventHandler<DataReceivedEventArgs> DataReceived;

		/// <summary>
		/// Opens the Bluetooth connection with the device.
		/// </summary>
		/// <exception cref="XBeeLibrary.Core.Exceptions.BluetoothAuthenticationException">If the
		/// BLE authentication process fails.</exception>
		/// <exception cref="XBeeLibrary.Core.Exceptions.InterfaceAlreadyOpenException">If the
		/// device is already connected.</exception>
		/// <exception cref="DigiIoTException">If there is any problem connecting the device.</exception>
		/// <seealso cref="IsConnected"/>
		/// <seealso cref="Disconnect"/>
		public void Connect();

		/// <summary>
		/// Closes the Bluetooth connection with the device.
		/// </summary>
		/// <exception cref="DigiIoTException">If there is any problem disconnecting the device.</exception>
		/// <seealso cref="IsConnected"/>
		/// <seealso cref="Connect"/>
		public void Disconnect();

		/// <summary>
		/// Sets the Bluetooth password of the Digi device in order to connect to it.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Connect"/> method.
		/// </remarks>
		/// <param name="password">The Bluetooth password of the device.</param>
		public void SetBluetoothPassword(string password);

		/// <summary>
		/// Sends the given data to the device.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="DigiIoTException">If there is any error sending the data.</exception>
		public void SendData(byte[] data);

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
		public byte[] ReadData();

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
		public byte[] ReadData(int timeout);

		/// <summary>
		/// Returns the Attribute MTU negotiated with the BLE Gatt server.
		/// </summary>
		/// <returns>The Attribute MTU negotiated with the BLE Gatt server.</returns>
		public int getAttMTU();
	}
}
