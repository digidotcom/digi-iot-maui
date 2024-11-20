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

namespace DigiIoT.Maui.Exceptions
{
	/// <summary>
	/// Generic Digi Remote Manager exception. This class indicates conditions that an application might want
	/// to catch. This exception can be thrown when any problem related to a Digi Remote Manager operation occurs.
	/// </summary>
	public class DRMException : Exception
	{
		/// <summary>
		/// HTTP status code associated with the error, if available.
		/// </summary>
		public int? StatusCode { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DRMException"/> class with a specified error
		/// message and optional HTTP status code.
		/// </summary>
		/// <param name="message">The error message describing the exception.</param>
		/// <param name="statusCode">The HTTP status code associated with the error (optional).</param>
		/// <param name="innerException">The inner exception, if any, that caused this exception (optional).</param>
		public DRMException(string message, int? statusCode = null, Exception innerException = null)
			: base(message, innerException)
		{
			StatusCode = statusCode;
		}
	}
}
