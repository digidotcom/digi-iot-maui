/*
 * Copyright 2024, Digi International Inc.
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

namespace DigiIoT.Maui.Services.Bluetooth
{
	/// <summary>
	/// Partial class that includes the custom Windows implementation of the Beacon
	/// advertise service.
	/// </summary>
	public partial class BeaconAdvertiseService
	{
		/// <summary>
		/// Starts advertising a custom beacon with the provided parameters.
		/// </summary>
		/// <param name="identifier">Identifier or local name of the beacon.</param>
		/// <param name="companyID">Company ID used in the manufacturer-specific data.</param>
		/// <param name="manufacturerData">Manufacturer-specific data to advertise.</param>
		/// <returns><code>true</code> if the process started successfully, <code>false</code>
		/// otherwise.</returns>
		public partial bool StartAdvertisingCustomBeacon(string identifier, byte[] companyID, byte[] manufacturerData)
		{
			throw new NotSupportedException("Service not implemented.");
		}

		/// <summary>
		/// Starts advertising an iBeacon with the provided parameters.
		/// </summary>
		/// <param name="identifier">Identifier or local name of the beacon.</param>
		/// <param name="uuid">UUID of the iBeacon.</param>
		/// <param name="major">Major value of the iBeacon.</param>
		/// <param name="minor">Minor value of the iBeacon.</param>
		/// <returns><code>true</code> if the process started successfully, <code>false</code>
		/// otherwise.</returns>
		public partial bool StartAdvertisingIBeacon(string identifier, string uuid, ushort major, ushort minor)
		{
			throw new NotSupportedException("Service not implemented.");
		}

		/// <summary>
		/// Stops advertising.
		/// </summary>
		public partial void StopAdvertising()
		{
			throw new NotSupportedException("Service not implemented.");
		}
	}
}