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
	/// Represents an entry in an XBee network allow list.
	/// </summary>
	public abstract class AllowListEntry
	{
		// Constants.
		private const string ERROR_INVALID_LABEL = "Invalid label format: {0}";

		// Variables.
		/// <summary>
		/// The list of registered parsers for creating <see cref="AllowListEntry"/> instances from raw label strings.
		/// To add support for a new protocol, add its <c>TryFromLabel</c> delegate here.
		/// </summary>
		private static readonly List<Func<string, AllowListEntry>> _parsers = new()
		{
			WiSunAllowListEntry.TryFromLabel
		};

		/// <summary>
		/// The list of registered parsers for creating <see cref="AllowListEntry"/> instances from API criteria.
		/// To add support for a new protocol, add its <c>TryFromCriteria</c> delegate here.
		/// </summary>
		private static readonly List<Func<Dictionary<string, string>, AllowListEntry>> _criteriaParsers = new()
		{
			WiSunAllowListEntry.TryFromCriteria
		};

		// Properties.
		/// <summary>
		/// The server-assigned identifier of the allow list entry. Null if not yet added to the network.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The protocol-specific criteria for this allow list entry.
		/// </summary>
		public abstract IAllowListCriteria Criteria { get; }

		/// <summary>
		/// Creates an <see cref="AllowListEntry"/> from a raw module label string.
		/// </summary>
		/// <param name="label">The raw label string scanned from a module.</param>
		/// <returns>A concrete <see cref="AllowListEntry"/> instance matching the label format.</returns>
		/// <exception cref="FormatException">If the label does not match any known format.</exception>
		public static AllowListEntry FromLabel(string label)
		{
			if (string.IsNullOrWhiteSpace(label))
				throw new FormatException(string.Format(ERROR_INVALID_LABEL, label));

			foreach (var tryParse in _parsers)
			{
				var entry = tryParse(label);
				if (entry is not null)
					return entry;
			}

			throw new FormatException(string.Format(ERROR_INVALID_LABEL, label));
		}

		/// <summary>
		/// Creates an <see cref="AllowListEntry"/> from a criteria dictionary returned by the API.
		/// </summary>
		/// <param name="criteria">The criteria dictionary from the API response.</param>
		/// <returns>A concrete <see cref="AllowListEntry"/> instance, or <c>null</c> if no parser matches.</returns>
		internal static AllowListEntry FromCriteria(Dictionary<string, string> criteria)
		{
			foreach (var tryParse in _criteriaParsers)
			{
				var entry = tryParse(criteria);
				if (entry is not null)
					return entry;
			}
			return null;
		}
	}
}