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
	/// Defines the criteria required to add an entry to an XBee network allow list.
	/// </summary>
	public interface IAllowListCriteria
	{
		// Properties.
		/// <summary>
		/// The protocol-specific parameters to include in the allow list request.
		/// </summary>
		Dictionary<string, object> Parameters { get; }

		// Methods.
		/// <summary>
		/// Returns whether this criteria is compatible with the given protocol.
		/// </summary>
		/// <param name="protocol">The XBee protocol to check compatibility with.</param>
		/// <returns><c>true</c> if compatible, <c>false</c> otherwise.</returns>
		bool IsCompatibleWith(XBeeProtocol protocol);
	}
}