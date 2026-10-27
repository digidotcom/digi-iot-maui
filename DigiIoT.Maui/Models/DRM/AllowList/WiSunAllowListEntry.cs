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
	/// Represents a Wi-SUN allow list entry.
	/// </summary>
	public class WiSunAllowListEntry : AllowListEntry
	{
		// Constants.
		private const string WISUN_LABEL_PREFIX = "WS.";
		private const string WISUN_OID_PREFIX = "1.3.6.1.4.1.332.11.20.";

		// Properties.
		/// <summary>
		/// The protocol-specific criteria for this Wi-SUN allow list entry.
		/// </summary>
		public override IAllowListCriteria Criteria { get; }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="WiSunAllowListEntry"/> object
		/// with the given parameters.
		/// </summary>
		/// <param name="hardwareType">The hardware type of the Wi-SUN module.</param>
		/// <param name="hardwareSerialNumber">The hardware serial number of the Wi-SUN module.</param>
		public WiSunAllowListEntry(string hardwareType, string hardwareSerialNumber)
		{
			Criteria = new WiSunAllowListCriteria(hardwareType, hardwareSerialNumber);
		}

		/// <summary>
		/// Attempts to create a <see cref="WiSunAllowListEntry"/> from a raw label string.
		/// </summary>
		/// <param name="label">The raw label string scanned from a module.</param>
		/// <returns>A new <see cref="WiSunAllowListEntry"/> if the label matches the Wi-SUN format,
		/// or <c>null</c> if it does not.</returns>
		internal static WiSunAllowListEntry TryFromLabel(string label)
		{
			if (!label.StartsWith(WISUN_LABEL_PREFIX))
				return null;

			string[] parts = label.Split(':');
			if (parts.Length != 3)
				return null;

			string oidSuffix = parts[0].Substring(WISUN_LABEL_PREFIX.Length);
			string hardwareType = WISUN_OID_PREFIX + oidSuffix;
			string hardwareSerialNumber = parts[1];
			// parts[2] is the EUI-64 — not used by the DRM allow list API.

			return new WiSunAllowListEntry(hardwareType, hardwareSerialNumber);
		}

		/// <summary>
		/// Attempts to create a <see cref="WiSunAllowListEntry"/> from a parameters dictionary.
		/// </summary>
		/// <param name="parameters">The criteria parameters dictionary from the API response.</param>
		/// <returns>A new <see cref="WiSunAllowListEntry"/> if the parameters match the Wi-SUN format,
		/// or <c>null</c> if they do not.</returns>
		internal static WiSunAllowListEntry TryFromParameters(Dictionary<string, string> parameters)
		{
			if (!parameters.TryGetValue(WiSunAllowListCriteria.KEY_HW_TYPE, out string hardwareType)
				|| !parameters.TryGetValue(WiSunAllowListCriteria.KEY_HW_SERIAL_NUMBER, out string hardwareSerialNumber))
				return null;

			return new WiSunAllowListEntry(hardwareType, hardwareSerialNumber);
		}

		/// <summary>
		/// Attempts to create a <see cref="WiSunAllowListEntry"/> from an allow list criteria.
		/// </summary>
		/// <param name="criteria">The allow list criteria.</param>
		/// <returns>A new <see cref="WiSunAllowListEntry"/> if the criteria is compatible with Wi-SUN
		/// and contains the required parameters, or <c>null</c> if it does not.</returns>
		internal static WiSunAllowListEntry TryFromCriteria(IAllowListCriteria criteria)
		{
			if (criteria is null || !criteria.IsCompatibleWith(XBeeProtocol.WiSun))
				return null;

			var parameters = criteria.Parameters;
			if (parameters is null
				|| !parameters.TryGetValue(WiSunAllowListCriteria.KEY_HW_TYPE, out object hardwareType)
				|| !parameters.TryGetValue(WiSunAllowListCriteria.KEY_HW_SERIAL_NUMBER, out object hardwareSerialNumber))
				return null;

			// Compatibility is only claimed by the criteria, so make sure the values are actually strings.
			if (hardwareType is not string hardwareTypeValue || hardwareSerialNumber is not string hardwareSerialNumberValue)
				return null;

			return new WiSunAllowListEntry(hardwareTypeValue, hardwareSerialNumberValue);
		}
	}
}