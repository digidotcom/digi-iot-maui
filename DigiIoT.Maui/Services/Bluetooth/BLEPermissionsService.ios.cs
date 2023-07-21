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

using CoreBluetooth;

namespace DigiIoT.Maui.Services.Bluetooth
{
	/// <summary>
	/// Partial class that includes the custom iOS implementation of the Bluetooth
	/// permissions service.
	/// </summary>
	public partial class BLEPermissionsService
	{
		/// <summary>
		/// Returns whether Bluetooth permissions where granted or not.
		/// </summary>
		/// <returns><c>true</c> if Bluetooth permissions where granted, <c>false</c> otherwise.</returns>
		public async partial Task<bool> RequestBLEPermissions()
		{
			// Initializing CBCentralManager will present the Bluetooth permission dialog.
			new CBCentralManager();
			PermissionStatus status;
			do
			{
				status = CheckPermissionAsync();
				await Task.Delay(200);
			} while (status == PermissionStatus.Unknown);
			return status == PermissionStatus.Granted;
		}

		/// <summary>
		/// Checks the Bluetooth permission status.
		/// </summary>
		/// <returns>The Bluetooth permission status.</returns>
		private PermissionStatus CheckPermissionAsync()
		{
			return CBManager.Authorization switch
			{
				CBManagerAuthorization.AllowedAlways => PermissionStatus.Granted,
				CBManagerAuthorization.Restricted => PermissionStatus.Restricted,
				CBManagerAuthorization.NotDetermined => PermissionStatus.Unknown,
				_ => PermissionStatus.Denied,
			};
		}
	}
}