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

using DigiIoT.Maui.Utils;
using DigiIoT.Maui.Exceptions;

namespace DigiIoT.Maui.Models.DRM
{
    /// <summary>
    /// Class representing a Digi Remote Manager account for cloud operations.
    /// </summary>
    public class DRMAccount
	{
		// Properties.
		/// <summary>
		/// Username of the Digi Remote Manager account.
		/// </summary>
		public string Username { get; }

		/// <summary>
		/// Password of the Digi Remote Manager account.
		/// </summary>
		public string Password { get; }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="DRMAccount"/> object with the given 
		/// parameters.
		/// </summary>
		/// <param name="username">Digi Remote Manager account username.</param>
		/// <param name="password">Digi Remote Manager account password.</param>
		/// <exception cref="ArgumentNullException">If username or password are null.</exception>
		public DRMAccount(string username, string password)
		{
			Username = username ?? throw new ArgumentNullException(nameof(username));
			Password = password ?? throw new ArgumentNullException(nameof(password));
		}

		/// <summary>
		/// Provisions a device to Digi Remote Manager using an optional install code.
		/// </summary>
		/// <param name="deviceID">The ID of the device to provision.</param>
		/// <param name="installCode">The optional install code for the device.</param>
		/// <returns>A task that returns the result of the provisioning operation.</returns>
		/// <exception cref="ArgumentNullException">If device ID is null.</exception>
		/// <exception cref="ArgumentException">If the device ID is invalid.</exception>
		/// <exception cref="DRMException">If the provisioning process fails.</exception>
		public async Task<DeviceProvisionResult> ProvisionDevice(string deviceID, string installCode = null)
		{
			return await DRMUtils.ProvisionDevice(this, deviceID, installCode);
		}

		/// <summary>
		/// Provisions a list of devices to Digi Remote Manager using an optional install code.
		/// </summary>
		/// <param name="devices">The list of devices to provision, each containing an ID and an optional install code.</param>
		/// <exception cref="ArgumentNullException">If the device list is null.</exception>
		/// <exception cref="ArgumentException">If the device list is empty.</exception>
		/// <exception cref="DRMException">If the request fails for all devices or the server response is invalid.</exception>
		public async Task<List<DeviceProvisionResult>> ProvisionDevices(List<DeviceProvisionRequest> devices)
		{
			return await DRMUtils.ProvisionDevices(this, devices);
		}

		/// <summary>
		/// Retrieves the list of XBee networks from Digi Remote Manager.
		/// </summary>
		/// <returns>A task that returns the list of XBee networks.</returns>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public async Task<List<XBeeNetwork>> ListNetworksAsync()
		{
			return await DRMUtils.ListNetworksAsync(this);
		}

		/// <summary>
		/// Retrieves all allow list entries for the given network.
		/// </summary>
		/// <param name="network">The XBee network to retrieve allow list entries for.</param>
		/// <returns>A task that returns the list of allow list entries.</returns>
		/// <exception cref="ArgumentNullException">If the network is null.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public async Task<List<AllowListEntry>> ListAllowListEntriesAsync(XBeeNetwork network)
		{
			return await DRMUtils.ListAllowListEntriesAsync(this, network);
		}

		/// <summary>
		/// Retrieves all allow list entries for the given network ID.
		/// </summary>
		/// <param name="networkId">The ID of the network to retrieve allow list entries for.</param>
		/// <returns>A task that returns the list of allow list entries.</returns>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public async Task<List<AllowListEntry>> ListAllowListEntriesAsync(long networkId)
		{
			return await DRMUtils.ListAllowListEntriesAsync(this, networkId);
		}

		/// <summary>
		/// Adds a list of allow list entries to the given network.
		/// Validates that each entry's criteria is compatible with the network protocol.
		/// </summary>
		/// <param name="network">The XBee network to add entries to.</param>
		/// <param name="entries">The list of allow list entries to add.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the network or entry list is null.</exception>
		/// <exception cref="ArgumentException">If the entry list is empty or any entry's criteria is incompatible with the network protocol.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public async Task<AllowListEntryResult> AddAllowListEntriesAsync(XBeeNetwork network, List<AllowListEntry> entries)
		{
			return await DRMUtils.AddAllowListEntriesAsync(this, network, entries);
		}

		/// <summary>
		/// Adds a list of allow list entries to the given network ID.
		/// </summary>
		/// <param name="networkId">The ID of the network to add entries to.</param>
		/// <param name="entries">The list of allow list entries to add.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the entry list is null.</exception>
		/// <exception cref="ArgumentException">If the entry list is empty.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public async Task<AllowListEntryResult> AddAllowListEntriesAsync(long networkId, List<AllowListEntry> entries)
		{
			return await DRMUtils.AddAllowListEntriesAsync(this, networkId, entries);
		}

		/// <summary>
		/// Removes a list of allow list entries from the given network.
		/// </summary>
		/// <param name="network">The XBee network to remove entries from.</param>
		/// <param name="entries">The list of allow list entries to remove. Entries must have a server-assigned ID.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the network or entry list is null.</exception>
		/// <exception cref="ArgumentException">If the entry list is empty or any entry has a null ID.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public async Task<AllowListEntryResult> RemoveAllowListEntriesAsync(XBeeNetwork network, List<AllowListEntry> entries)
		{
			return await DRMUtils.RemoveAllowListEntriesAsync(this, network, entries);
		}

		/// <summary>
		/// Removes allow list entries by ID from the given network.
		/// </summary>
		/// <param name="network">The XBee network to remove entries from.</param>
		/// <param name="entryIds">The list of allow list entry IDs to remove.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the network or entry ID list is null.</exception>
		/// <exception cref="ArgumentException">If the entry ID list is empty.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public async Task<AllowListEntryResult> RemoveAllowListEntriesAsync(XBeeNetwork network, List<long> entryIds)
		{
			return await DRMUtils.RemoveAllowListEntriesAsync(this, network, entryIds);
		}

		/// <summary>
		/// Removes a list of allow list entries from the given network ID.
		/// </summary>
		/// <param name="networkId">The ID of the network to remove entries from.</param>
		/// <param name="entries">The list of allow list entries to remove. Entries must have a server-assigned ID.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the entry list is null.</exception>
		/// <exception cref="ArgumentException">If the entry list is empty or any entry has a null ID.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public async Task<AllowListEntryResult> RemoveAllowListEntriesAsync(long networkId, List<AllowListEntry> entries)
		{
			return await DRMUtils.RemoveAllowListEntriesAsync(this, networkId, entries);
		}

		/// <summary>
		/// Removes allow list entries by ID from the given network ID.
		/// </summary>
		/// <param name="networkId">The ID of the network to remove entries from.</param>
		/// <param name="entryIds">The list of allow list entry IDs to remove.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the entry ID list is null.</exception>
		/// <exception cref="ArgumentException">If the entry ID list is empty.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public async Task<AllowListEntryResult> RemoveAllowListEntriesAsync(long networkId, List<long> entryIds)
		{
			return await DRMUtils.RemoveAllowListEntriesAsync(this, networkId, entryIds);
		}
	}
}
