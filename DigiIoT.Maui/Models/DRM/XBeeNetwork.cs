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

using Newtonsoft.Json;

namespace DigiIoT.Maui.Models.DRM
{
	/// <summary>
	/// Represents an XBee network returned from Digi Remote Manager.
	/// </summary>
	public class XBeeNetwork
	{
		// Properties.
		/// <summary>
		/// The unique identifier of the network.
		/// </summary>
		[JsonProperty("id")]
		public long Id { get; set; }

		/// <summary>
		/// The display name of the network.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The protocol of the network.
		/// </summary>
		[JsonProperty("protocol")]
		public XBeeProtocol Protocol { get; set; }

		/// <summary>
		/// The authorization mode of the network.
		/// </summary>
		[JsonProperty("authorization_mode")]
		public string Authorization { get; set; }
	}
}