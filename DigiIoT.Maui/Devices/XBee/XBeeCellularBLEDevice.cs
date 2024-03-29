﻿/*
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
using XBeeLibrary.Core.Models;

namespace DigiIoT.Maui.Devices.XBee
{
	/// <summary>
	/// This class represents an XBee Cellular device with Bluetooth Low Energy (BLE) connectivity.
	/// </summary>
	/// <seealso cref="XBeeDigiMeshBLEDevice"/>
	/// <seealso cref="XBee802BLEDevice"/>
	/// <seealso cref="XBeeBLEDevice"/>
	/// <seealso cref="XBeeZigbeeBLEDevice"/>
	public class XBeeCellularBLEDevice : XBeeBLEDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeCellularBLEDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="XBeeBLEDevice.Connect"/> method,
		/// either through this constructor or the <see cref="XBeeBLEDevice.SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="device">Bluetooth device to connect to.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <seealso cref="IDevice"/>
		public XBeeCellularBLEDevice(IDevice device, string password)
			: base(device, password) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeCellularBLEDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="XBeeBLEDevice.Connect"/> method,
		/// either through this constructor or the <see cref="XBeeBLEDevice.SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="deviceAddress">The address of the Bluetooth device. It must follow the
		/// format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c>.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <exception cref="ArgumentException">If <paramref name="deviceAddress"/> does not follow
		/// the format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c>.</exception>
		public XBeeCellularBLEDevice(string deviceAddress, string password)
			: base(deviceAddress, password) { }

		// Properties.
		/// <summary>
		/// The protocol of the XBee device.
		/// </summary>
		/// <seealso cref="XBeeProtocol.CELLULAR"/>
		public override XBeeProtocol XBeeProtocol => XBeeProtocol.CELLULAR;

		/// <summary>
		/// The IMEI address of the cellular device.
		/// </summary>
		public XBeeIMEIAddress IMEIAddress
		{
			get
			{
				_imeiAddress ??= new XBeeIMEIAddress(XBee64BitAddr.Value);
				return _imeiAddress;
			}
		}

		// Variables.
		private XBeeIMEIAddress _imeiAddress;
	}
}
