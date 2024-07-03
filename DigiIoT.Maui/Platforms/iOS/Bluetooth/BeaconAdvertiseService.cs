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

using CoreBluetooth;
using CoreLocation;
using Foundation;
using UIKit;

namespace DigiIoT.Maui.Services.Bluetooth
{
	/// <summary>
	/// Partial class that includes the custom iOS implementation of the Beacon
	/// advertise service.
	/// </summary>
	public partial class BeaconAdvertiseService
	{
		// Constants.
		private const string ADVERT_BEACON_KEY = "kCBAdvDataAppleBeaconKey";

		// Variables.
		private CBPeripheralManager _peripheralManager;
		private NSDictionary _advertisementData;

		/// <summary>
		/// Starts advertising a custom beacon with the provided parameters.
		/// </summary>
		/// <param name="identifier">Identifier or local name of the beacon.</param>
		/// <param name="companyID">Company ID used in the manufacturer-specific data.</param>
		/// <param name="manufacturerData">Manufacturer-specific data to advertise.</param>
		/// <returns><code>true</code> if the process started successfully, <code>false</code>
		/// otherwise.</returns>
		public partial bool StartAdvertisingCustomBeacon(string identifier, byte[] companyID, byte[] manufacturerData)
		{
			// The way a custom beacon would be generated in iOS is as follows:
			//
			//// Convert the byte array to NSData
			//var nsData = NSData.FromArray([.. companyID, .. manufacturerData]);
			//
			//// Set up the advertisement data
			//_advertisementData = new NSDictionary(
			//	CBAdvertisement.DataLocalNameKey, identifier,
			//	CBAdvertisement.DataManufacturerDataKey, nsData
			//);
			//
			// However, the 'DataManufacturerDataKey' tag is not allowed for advertisement, so
			// custom beacons are not available in iOS.

			throw new NotSupportedException("Manufacturer data AD is not supported in iOS.");
		}

		/// <summary>
		/// Starts advertising an iBeacon with the provided parameters.
		/// </summary>
		/// <param name="identifier">Identifier or local name of the beacon.</param>
		/// <param name="uuid">UUID of the iBeacon.</param>
		/// <param name="major">Major value of the iBeacon.</param>
		/// <param name="minor">Minor value of the iBeacon.</param>
		/// <returns><code>true</code> if the process started successfully, <code>false</code>
		/// otherwise.</returns>
		public partial bool StartAdvertisingIBeacon(string identifier, string uuid, ushort major, ushort minor)
		{
			// Initialize vars.
			var proximityUUID = new NSUuid(uuid);

			if (UIDevice.CurrentDevice.CheckSystemVersion(17, 0))
			{
				// For iOS 17.0 and later
				// 'CLBeaconIdentityConstraint' and 'CLBeaconRegion' are deprecated for iOS 17
				// and later. As there is not a clear API to generate the iBeacon payload now,
				// generate it manually.
				var beaconUUIDBytes = proximityUUID.GetBytes();
				var majorBytes = BitConverter.GetBytes(major);
				var minorBytes = BitConverter.GetBytes(minor);

				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(majorBytes);
					Array.Reverse(minorBytes);
				}

				var beaconData = new byte[21];
				Array.Copy(beaconUUIDBytes, 0, beaconData, 0, 16);
				Array.Copy(majorBytes, 0, beaconData, 16, 2);
				Array.Copy(minorBytes, 0, beaconData, 18, 2);
				Array.Copy(TX_POWER_DEFAULT, 0, beaconData, 20, 1);

				var beaconNSData = NSData.FromArray(beaconData);

				_advertisementData = new NSDictionary(
					CBAdvertisement.DataLocalNameKey, identifier,
					new NSString(ADVERT_BEACON_KEY), beaconNSData
				);
			}
			else if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
			{
				// For iOS 13.0 to 16.x
				var beaconIdentityConstraint = new CLBeaconIdentityConstraint(proximityUUID, major, minor);
				var beaconRegion = new CLBeaconRegion(beaconIdentityConstraint, identifier);
				_advertisementData = beaconRegion.GetPeripheralData(null);
			}
			else
			{
				// For iOS 12.x and earlier
				var beaconRegion = new CLBeaconRegion(proximityUUID, major, minor, identifier);
				_advertisementData = beaconRegion.GetPeripheralData(null);
			}

			// Start advertising data.
			return StartAdvertisingInternal();
		}

		/// <summary>
		/// Starts advertising data.
		/// </summary>
		/// <returns><code>true</code> if the process started successfully, <code>false</code>
		/// otherwise.</returns>
		private bool StartAdvertisingInternal()
		{
			// Initialize peripheral manager if not already done.
			if (_peripheralManager == null)
			{
				_peripheralManager = new CBPeripheralManager();
				_peripheralManager.StateUpdated += (sender, e) =>
				{
					if (_peripheralManager.State == CBManagerState.PoweredOn && _advertisementData != null)
					{
						_peripheralManager.StartAdvertising(_advertisementData);
					}
				};
			}

			if (_peripheralManager.State == CBManagerState.PoweredOn && _advertisementData != null)
			{
				_peripheralManager.StartAdvertising(_advertisementData);
			}

			return true;
		}

		/// <summary>
		/// Stops advertising.
		/// </summary>
		public partial void StopAdvertising()
		{
			_peripheralManager?.StopAdvertising();
			// Reset data so that it does not start advertising automatically if the
			// interface goes ON.
			_advertisementData = null;
		}
	}
}