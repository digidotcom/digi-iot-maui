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
    public partial class BLEPermissionsService
    {
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
            switch (CBManager.Authorization)
            {
                case CBManagerAuthorization.AllowedAlways:
                    return PermissionStatus.Granted;
                case CBManagerAuthorization.Restricted:
                    return PermissionStatus.Restricted;
                case CBManagerAuthorization.NotDetermined:
                    return PermissionStatus.Unknown;
                default:
                    return PermissionStatus.Denied;
            }
        }
    }
}