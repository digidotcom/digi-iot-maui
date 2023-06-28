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

using Plugin.BLE.Abstractions.Contracts;

namespace DigiIoT.Maui.Devices.ConnectCore
{
	public class ConnectCoreBLEDevice : DigiDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ConnectCoreBLEDevice"/> object with
		/// the given parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="DigiDevice.Connect"/> method,
		/// either through this constructor or the <see cref="DigiDevice.SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="device">Bluetooth device to connect to.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <seealso cref="IDevice"/>
		public ConnectCoreBLEDevice(IDevice device, string password)
			: base(device, password) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ConnectCoreBLEDevice"/> object with
		/// the given parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="DigiDevice.Connect"/> method,
		/// either through this constructor or the <see cref="DigiDevice.SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="deviceAddress">The address or GUID of the Bluetooth device. It must follow the
		/// format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c> for the address or
		/// <c>01234567-0123-0123-0123-0123456789AB</c> for the GUID.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <exception cref="ArgumentException">If <paramref name="deviceAddress"/> does not follow
		/// the format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c> or
		/// <c>01234567-0123-0123-0123-0123456789AB</c>.</exception>
		public ConnectCoreBLEDevice(string deviceAddress, string password)
			: base(deviceAddress, password) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ConnectCoreBLEDevice"/> object with
		/// the given parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="DigiDevice.Connect"/> method,
		/// either through this constructor or the <see cref="DigiDevice.SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="deviceGuid">The Bluetooth device GUID.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <seealso cref="Guid"/>
		public ConnectCoreBLEDevice(Guid deviceGuid, string password)
			: base(deviceGuid, password) { }
	}
}
