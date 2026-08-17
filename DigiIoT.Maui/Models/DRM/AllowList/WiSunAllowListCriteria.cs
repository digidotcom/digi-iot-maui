/*
 * Copyright 2026, Digi International Inc.
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

namespace DigiIoT.Maui.Models.DRM
{
	/// <summary>
	/// Represents the allow list criteria for a Wi-SUN module.
	/// </summary>
	public class WiSunAllowListCriteria : IAllowListCriteria
	{
		// Properties.
		/// <summary>
		/// The hardware type of the Wi-SUN module.
		/// </summary>
		public string HardwareType { get; }

		/// <summary>
		/// The hardware serial number of the Wi-SUN module.
		/// </summary>
		public string HardwareSerialNumber { get; }

		/// <summary>
		/// The protocol-specific parameters to include in the allow list request.
		/// </summary>
		public Dictionary<string, object> Parameters => new Dictionary<string, object>
		{
			{ "hw_type", HardwareType },
			{ "hw_serial_number", HardwareSerialNumber }
		};

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="WiSunAllowListCriteria"/> object
		/// with the given parameters.
		/// </summary>
		/// <param name="hardwareType">The hardware type of the Wi-SUN module.</param>
		/// <param name="hardwareSerialNumber">The hardware serial number of the Wi-SUN module.</param>
		public WiSunAllowListCriteria(string hardwareType, string hardwareSerialNumber)
		{
			HardwareType = hardwareType;
			HardwareSerialNumber = hardwareSerialNumber;
		}

		/// <summary>
		/// Returns whether this criteria is compatible with the given protocol.
		/// </summary>
		/// <param name="protocol">The XBee protocol to check compatibility with.</param>
		/// <returns><c>true</c> if the protocol is <see cref="XBeeProtocol.WiSun"/>, <c>false</c> otherwise.</returns>
		public bool IsCompatibleWith(XBeeProtocol protocol)
		{
			return protocol == XBeeProtocol.WiSun;
		}
	}
}