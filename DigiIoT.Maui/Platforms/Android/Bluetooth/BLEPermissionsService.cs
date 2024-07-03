/*
 * Copyright 2023,2024, Digi International Inc.
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

using Android;
using Android.OS;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace DigiIoT.Maui.Services.Bluetooth
{
	/// <summary>
	/// Partial class that includes the custom Android implementation of the Bluetooth
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
			if (Build.VERSION.SdkInt < BuildVersionCodes.S)
			{
				return true;
			}
			PermissionStatus status = await RequestAsync<BLEPermission>();
			return status == PermissionStatus.Granted;
		}
	}

	/// <summary>
	/// Class that lists the BLE permissions that should be requested.
	/// </summary>
	internal class BLEPermission : BasePlatformPermission
	{
		public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
		{
			(Manifest.Permission.BluetoothConnect, true),
			(Manifest.Permission.BluetoothScan, true),
			(Manifest.Permission.BluetoothAdvertise, true)
		}.ToArray();
	}
}