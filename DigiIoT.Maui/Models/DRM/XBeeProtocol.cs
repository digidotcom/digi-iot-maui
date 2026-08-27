/*
 * Copyright 2024-2026, Digi International Inc.
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
	/// Enumerates the supported XBee network protocols.
	/// </summary>
	public enum XBeeProtocol
	{
		/// <summary>
		/// Unknown or unsupported protocol.
		/// </summary>
		Unknown = -1,

		/// <summary>
		/// Zigbee protocol.
		/// </summary>
		Zigbee = 0,

		/// <summary>
		/// DigiMesh protocol.
		/// </summary>
		DigiMesh = 1,

		/// <summary>
		/// DigiMeshSub protocol.
		/// </summary>
		DigiMeshSub = 2,

		/// <summary>
		/// 802.15.4 protocol.
		/// </summary>
		Raw802 = 3,

		/// <summary>
		/// Wi-SUN protocol.
		/// </summary>
		WiSun = 4
	}

	public static class XBeeProtocolExtensions
	{
		/// <summary>
		/// Retrieves the <see cref="XBeeProtocol"/> for the given API identifier string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="identifier">The protocol string returned by the API.</param>
		/// <returns>The corresponding <see cref="XBeeProtocol"/>, or <see cref="XBeeProtocol.Unknown"/>
		/// if the identifier is not recognized.</returns>
		public static XBeeProtocol Get(string identifier)
		{
			var values = Enum.GetValues(typeof(XBeeProtocol)).OfType<XBeeProtocol>();

			foreach (var value in values)
			{
				if (value.ToString().Equals(identifier, StringComparison.OrdinalIgnoreCase))
					return value;
			}

			return XBeeProtocol.Unknown;
		}
	}
}