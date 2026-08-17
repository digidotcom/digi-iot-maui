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

using DigiIoT.Maui.Exceptions;
using DigiIoT.Maui.Models.DRM;
using DigiIoT.Maui.Types;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DigiIoT.Maui.Utils
{
    /// <summary>
    /// Class containing Digi Remote Manager related operations.
    /// </summary>
    public class DRMUtils
	{
		// Constants.
		private const string URL_REMOTE_MANAGER = "https://remotemanager.digi.com";
		private const string ENDPOINT_DEVICES_INV = $"{URL_REMOTE_MANAGER}/ws/v1/devices/inventory";
		private const string ENDPOINT_NETWORKS_INV = $"{URL_REMOTE_MANAGER}/ws/v1/xbee/networks/inventory";
		private const string ENDPOINT_ALLOW_LIST = $"{ENDPOINT_NETWORKS_INV}/{{0}}/allow_list";

		private static readonly Regex DEVICE_ID_REGEX = new Regex(
			@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{8}-[0-9a-fA-F]{8}-[0-9a-fA-F]{8}$",
			RegexOptions.Compiled);


		private const string ERROR_DEVICE_ID_EMPTY = "Device ID cannot be null or empty.";
		private const string ERROR_DEVICE_ID_INVALID = "Device ID format is not valid. Expected format is 00000000-00000000-00000000-00000000.";
		private const string ERROR_DEVICE_LIST_EMPTY = "Device list cannot be null or empty.";
		private const string ERROR_ENTRY_LIST_EMPTY = "Entry list cannot be null or empty.";
		private const string ERROR_ENTRY_IDS_EMPTY = "Entry ID list cannot be null or empty.";
		private const string ERROR_ENTRY_ID_NULL = "One or more entries have a null ID. Ensure entries were retrieved from the API before removing.";
		private const string ERROR_PROVISIONING_FAILED = "Device provisioning failed. Status Code: {0}, Details: {1}";
        private const string ERROR_NETWORK = "A network error occurred while communicating with Digi Remote Manager.";
        private const string ERROR_LIST_NETWORKS_FAILED = "Failed to retrieve networks. Status Code: {0}, Details: {1}";
		private const string ERROR_LIST_ALLOWLIST_FAILED = "Failed to retrieve allow list entries. Status Code: {0}, Details: {1}";
		private const string ERROR_CRITERIA_INCOMPATIBLE = "One or more entries have criteria incompatible with the network protocol.";
		private const string ERROR_OTHER = "An unexpected error occurred.";

		/// <summary>
		/// Provisions a single device to Digi Remote Manager using a <see cref="DRMAccount"/> and an optional install code.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="deviceId">The ID of the device to provision.</param>
		/// <param name="installCode">The optional install code for the device.</param>
		/// <returns>A task that returns the result of the provisioning operation.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account or device ID is null.</exception>
		/// <exception cref="ArgumentException">If the device ID is invalid.</exception>
		/// <exception cref="DRMException">If the provisioning process fails.</exception>
		public static async Task<DeviceProvisionResult> ProvisionDevice(DRMAccount drmAccount, string deviceId, string installCode = null)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			return await ProvisionDevice(drmAccount.Username, drmAccount.Password, deviceId, installCode);
		}

		/// <summary>
		/// Provisions a single device to Digi Remote Manager using raw credentials and an optional install code.
		/// </summary>
		/// <param name="username">The username for Digi Remote Manager authentication.</param>
		/// <param name="password">The password for Digi Remote Manager authentication.</param>
		/// <param name="deviceId">The ID of the device to provision.</param>
		/// <param name="installCode">The optional install code for the device.</param>
		/// <returns>A task that returns the result of the provisioning operation.</returns>
		/// <exception cref="ArgumentNullException">If the username, password, or device ID is null.</exception>
		/// <exception cref="ArgumentException">If the device ID is invalid.</exception>
		/// <exception cref="DRMException">If the provisioning process fails.</exception>
		public static async Task<DeviceProvisionResult> ProvisionDevice(string username, string password, string deviceId, string installCode = null)
		{
			ValidateDeviceId(deviceId);

			var deviceRequest = new DeviceProvisionRequest { Id = deviceId, InstallCode = installCode ?? string.Empty };
			var results = await ProvisionDevices(username, password, new List<DeviceProvisionRequest> { deviceRequest });

			// Since we are provisioning one device, return the first result.
			return results[0];
		}

		/// <summary>
		/// Provisions multiple devices to Digi Remote Manager using a <see cref="DRMAccount"/>.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="devices">The list of devices to provision, each containing an ID and an optional install code.</param>
		/// <returns>A task that returns a list of provisioning results for each device.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account or device list is null.</exception>
		/// <exception cref="ArgumentException">If the device list is empty.</exception>
		/// <exception cref="DRMException">If the request fails for all devices or the server response is invalid.</exception>
		public static async Task<List<DeviceProvisionResult>> ProvisionDevices(DRMAccount drmAccount, List<DeviceProvisionRequest> devices)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			return await ProvisionDevices(drmAccount.Username, drmAccount.Password, devices);
		}

		/// <summary>
		/// Provisions multiple devices to Digi Remote Manager using raw credentials.
		/// </summary>
		/// <param name="drmUser">The username for Digi Remote Manager authentication.</param>
		/// <param name="drmPassword">The password for Digi Remote Manager authentication.</param>
		/// <param name="devices">The list of devices to provision, each containing an ID and an optional install code.</param>
		/// <returns>A task that returns a list of provisioning results for each device.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account or device list is null.</exception>
		/// <exception cref="ArgumentException">If the device list is empty.</exception>
		/// <exception cref="DRMException">If the request fails for all devices or the server response is invalid.</exception>
		public static async Task<List<DeviceProvisionResult>> ProvisionDevices(string drmUser, string drmPassword, List<DeviceProvisionRequest> devices)
		{
			if (devices == null || devices.Count == 0)
				throw new ArgumentException(ERROR_DEVICE_LIST_EMPTY, nameof(devices));

			using (HttpClient client = CreateAuthenticatedClient(drmUser, drmPassword))
			{
				try
				{
					string jsonPayload = JsonConvert.SerializeObject(devices, Formatting.None, new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore
					});

					HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
					HttpResponseMessage response = await client.PostAsync(ENDPOINT_DEVICES_INV, content);

					string responseBody = await response.Content.ReadAsStringAsync();

					if (response.StatusCode == HttpStatusCode.Created) // 201
					{
						// All devices were provisioned successfully. Create results for each one.
						var results = new List<DeviceProvisionResult>();

						foreach (var device in devices)
						{
							results.Add(new DeviceProvisionResult
							{
								DeviceId = device.Id,
								IsSuccess = true,
								ErrorMessage = null,
								ErrorCode = null
							});
						}
						return results;
					}
					else if (response.StatusCode == HttpStatusCode.MultiStatus) // 207
					{
						// Handle partial success or failure for multiple devices.
						var responseObject = JsonConvert.DeserializeObject<DeviceProvisionResponse>(responseBody);
						var results = new List<DeviceProvisionResult>();

						foreach (var deviceResult in responseObject.List)
						{
							results.Add(new DeviceProvisionResult
							{
								DeviceId = deviceResult.ErrorContext?.Id ?? deviceResult.Id,
								IsSuccess = deviceResult.ErrorStatus == null,
								ErrorMessage = deviceResult.ErrorMessage,
								ErrorCode = deviceResult.ErrorStatus
							});
						}
						return results;
					}
					else
					{
						throw new DRMException(string.Format(ERROR_PROVISIONING_FAILED,
							response.StatusCode, ExtractErrorMessage(responseBody)),
							(int)response.StatusCode);
					}
                }
                catch (HttpRequestException ex)
                {
                    throw new DRMException(ERROR_NETWORK, null, ex);
                }
                catch (DRMException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DRMException(ERROR_OTHER, null, ex);
                }
            }
        }

		/// <summary>
		/// Retrieves the list of XBee networks from Digi Remote Manager using a <see cref="DRMAccount"/>.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <returns>A task that returns the list of XBee networks.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account is null.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<List<XBeeNetwork>> ListNetworksAsync(DRMAccount drmAccount)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			return await ListNetworksAsync(drmAccount.Username, drmAccount.Password);
		}

		/// <summary>
		/// Retrieves the list of XBee networks from Digi Remote Manager using raw credentials.
		/// </summary>
		/// <param name="drmUser">The username for Digi Remote Manager authentication.</param>
		/// <param name="drmPassword">The password for Digi Remote Manager authentication.</param>
		/// <returns>A task that returns the list of XBee networks.</returns>
		/// <exception cref="ArgumentNullException">If the username or password is null.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<List<XBeeNetwork>> ListNetworksAsync(string drmUser, string drmPassword)
		{
			using (HttpClient client = CreateAuthenticatedClient(drmUser, drmPassword))
			{
				try
				{
					var networks = new List<XBeeNetwork>();
					string cursor = null;

					while (true)
					{
						string url = ENDPOINT_NETWORKS_INV;
						if (cursor != null)
							url += $"?cursor={cursor}";

						HttpResponseMessage response = await client.GetAsync(url);
						string responseBody = await response.Content.ReadAsStringAsync();

						if (response.IsSuccessStatusCode)
						{
							var responseObject = JsonConvert.DeserializeObject<XBeeNetworkListResponse>(responseBody);

							if (responseObject?.List != null)
							{
								foreach (var item in responseObject.List)
								{
									networks.Add(new XBeeNetwork
									{
										Id = item.Id,
										Name = item.Name,
										Protocol = XBeeProtocolExtensions.Get(item.Protocol),
										Authorization = item.AuthorizationMode
									});
								}
							}

							if (string.IsNullOrEmpty(responseObject?.Cursor))
								break;

							cursor = responseObject.Cursor;
						}
						else
						{
							throw new DRMException(
								string.Format(ERROR_LIST_NETWORKS_FAILED, response.StatusCode, ExtractErrorMessage(responseBody)),
								(int)response.StatusCode);
						}
					}

					return networks;
				}
				catch (HttpRequestException ex)
				{
					throw new DRMException(ERROR_NETWORK, null, ex);
				}
				catch (DRMException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new DRMException(ERROR_OTHER, null, ex);
				}
			}
		}

		/// <summary>
		/// Retrieves all allow list entries for the given network using a <see cref="DRMAccount"/>.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="network">The XBee network to retrieve allow list entries for.</param>
		/// <returns>A task that returns the list of allow list entries.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account or network is null.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<List<AllowListEntry>> ListAllowListEntriesAsync(DRMAccount drmAccount, XBeeNetwork network)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			if (network == null)
				throw new ArgumentNullException(nameof(network));

			return await ListAllowListEntriesAsync(drmAccount.Username, drmAccount.Password, network.Id);
		}

		/// <summary>
		/// Retrieves all allow list entries for the given network ID using a <see cref="DRMAccount"/>.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="networkId">The ID of the network to retrieve allow list entries for.</param>
		/// <returns>A task that returns the list of allow list entries.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account is null.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<List<AllowListEntry>> ListAllowListEntriesAsync(DRMAccount drmAccount, long networkId)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			return await ListAllowListEntriesAsync(drmAccount.Username, drmAccount.Password, networkId);
		}

		/// <summary>
		/// Retrieves all allow list entries for the given network ID using raw credentials.
		/// Iterates through all pages before returning the complete list.
		/// </summary>
		/// <param name="drmUser">The username for Digi Remote Manager authentication.</param>
		/// <param name="drmPassword">The password for Digi Remote Manager authentication.</param>
		/// <param name="networkId">The ID of the network to retrieve allow list entries for.</param>
		/// <returns>A task that returns the complete list of allow list entries.</returns>
		/// <exception cref="ArgumentNullException">If the username or password is null.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<List<AllowListEntry>> ListAllowListEntriesAsync(string drmUser, string drmPassword, long networkId)
		{
			using (HttpClient client = CreateAuthenticatedClient(drmUser, drmPassword))
			{
				try
				{
					var entries = new List<AllowListEntry>();
					string cursor = null;

					while (true)
					{
						string url = string.Format(ENDPOINT_ALLOW_LIST, networkId);
						if (cursor != null)
							url += $"?cursor={cursor}";

						HttpResponseMessage response = await client.GetAsync(url);
						string responseBody = await response.Content.ReadAsStringAsync();

						if (response.IsSuccessStatusCode)
						{
							var responseObject = JsonConvert.DeserializeObject<AllowListResponse>(responseBody);

							if (responseObject?.List != null)
							{
								foreach (var item in responseObject.List)
								{
									var entry = AllowListEntry.FromCriteria(item.Criteria);
									if (entry == null) continue;
									entry.Id = item.Id;
									entries.Add(entry);
								}
							}

							if (string.IsNullOrEmpty(responseObject?.Cursor))
								break;

							cursor = responseObject.Cursor;
						}
						else
						{
							throw new DRMException(
								string.Format(ERROR_LIST_ALLOWLIST_FAILED, response.StatusCode, ExtractErrorMessage(responseBody)),
								(int)response.StatusCode);
						}
					}

					return entries;
				}
				catch (HttpRequestException ex)
				{
					throw new DRMException(ERROR_NETWORK, null, ex);
				}
				catch (DRMException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new DRMException(ERROR_OTHER, null, ex);
				}
			}
		}

		/// <summary>
		/// Adds a list of allow list entries to the given network using a <see cref="DRMAccount"/>.
		/// Validates that each entry's criteria is compatible with the network protocol.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="network">The XBee network to add entries to.</param>
		/// <param name="entries">The list of allow list entries to add.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account, network, or entry list is null.</exception>
		/// <exception cref="ArgumentException">If the entry list is empty or any entry's criteria is incompatible with the network protocol.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<AllowListEntryResult> AddAllowListEntriesAsync(DRMAccount drmAccount, XBeeNetwork network, List<AllowListEntry> entries)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			if (network == null)
				throw new ArgumentNullException(nameof(network));

			if (entries == null || entries.Count == 0)
				throw new ArgumentException(ERROR_ENTRY_LIST_EMPTY, nameof(entries));

			if (entries.Any(e => !e.Criteria.IsCompatibleWith(network.Protocol)))
				throw new ArgumentException(ERROR_CRITERIA_INCOMPATIBLE, nameof(entries));

			return await AddAllowListEntriesAsync(drmAccount.Username, drmAccount.Password, network.Id, entries);
		}

		/// <summary>
		/// Adds a list of allow list entries to the given network ID using a <see cref="DRMAccount"/>.
		/// Compatibility checking is skipped when only a network ID is provided.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="networkId">The ID of the network to add entries to.</param>
		/// <param name="entries">The list of allow list entries to add.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account or entry list is null.</exception>
		/// <exception cref="ArgumentException">If the entry list is empty.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<AllowListEntryResult> AddAllowListEntriesAsync(DRMAccount drmAccount, long networkId, List<AllowListEntry> entries)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			return await AddAllowListEntriesAsync(drmAccount.Username, drmAccount.Password, networkId, entries);
		}

		/// <summary>
		/// Adds a list of allow list entries to the given network ID using raw credentials.
		/// </summary>
		/// <param name="drmUser">The username for Digi Remote Manager authentication.</param>
		/// <param name="drmPassword">The password for Digi Remote Manager authentication.</param>
		/// <param name="networkId">The ID of the network to add entries to.</param>
		/// <param name="entries">The list of allow list entries to add.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the username or password is null.</exception>
		/// <exception cref="ArgumentException">If the entry list is null or empty.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<AllowListEntryResult> AddAllowListEntriesAsync(string drmUser, string drmPassword, long networkId, List<AllowListEntry> entries)
		{
			if (entries == null || entries.Count == 0)
				throw new ArgumentException(ERROR_ENTRY_LIST_EMPTY, nameof(entries));

			using (HttpClient client = CreateAuthenticatedClient(drmUser, drmPassword))
			{
				try
				{
					var body = entries.Select(e => e.Criteria.Parameters).ToList();
					string jsonPayload = JsonConvert.SerializeObject(body, Formatting.None);
					HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

					HttpResponseMessage response = await client.PostAsync(string.Format(ENDPOINT_ALLOW_LIST, networkId), content);
					string responseBody = await response.Content.ReadAsStringAsync();

					if (response.IsSuccessStatusCode)
					{
						return new AllowListEntryResult { IsSuccess = true };
					}
					else
					{
						return new AllowListEntryResult
						{
							IsSuccess = false,
							ErrorMessage = ExtractErrorMessage(responseBody),
							ErrorCode = (int)response.StatusCode
						};
					}
				}
				catch (HttpRequestException ex)
				{
					throw new DRMException(ERROR_NETWORK, null, ex);
				}
				catch (DRMException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new DRMException(ERROR_OTHER, null, ex);
				}
			}
		}

		/// <summary>
		/// Validates the provided device ID.
		/// </summary>
		/// <param name="deviceID">Device ID to validate.</param>
		/// <exception cref="ArgumentException">If the provided device ID is not valid.</exception>
		private static void ValidateDeviceId(string deviceID)
		{
			if (string.IsNullOrEmpty(deviceID))
				throw new ArgumentException(ERROR_DEVICE_ID_EMPTY, nameof(deviceID));

			if (!DEVICE_ID_REGEX.IsMatch(deviceID))
				throw new ArgumentException(ERROR_DEVICE_ID_INVALID, nameof(deviceID));
		}

		/// <summary>
		/// Removes a list of allow list entries from the given network using a <see cref="DRMAccount"/>.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="network">The XBee network to remove entries from.</param>
		/// <param name="entries">The list of allow list entries to remove. Entries must have a server-assigned ID.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account, network, or entry list is null.</exception>
		/// <exception cref="ArgumentException">If the entry list is empty or any entry has a null ID.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<AllowListEntryResult> RemoveAllowListEntriesAsync(DRMAccount drmAccount, XBeeNetwork network, List<AllowListEntry> entries)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			if (network == null)
				throw new ArgumentNullException(nameof(network));

			if (entries == null || entries.Count == 0)
				throw new ArgumentException(ERROR_ENTRY_LIST_EMPTY, nameof(entries));

			if (entries.Any(e => e.Id == null))
				throw new ArgumentException(ERROR_ENTRY_ID_NULL, nameof(entries));

			return await RemoveAllowListEntriesAsync(drmAccount.Username, drmAccount.Password, network.Id, entries.Select(e => e.Id.Value).ToList());
		}

		/// <summary>
		/// Removes allow list entries by ID from the given network using a <see cref="DRMAccount"/>.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="network">The XBee network to remove entries from.</param>
		/// <param name="entryIds">The list of allow list entry IDs to remove.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account, network, or entry ID list is null.</exception>
		/// <exception cref="ArgumentException">If the entry ID list is empty.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<AllowListEntryResult> RemoveAllowListEntriesAsync(DRMAccount drmAccount, XBeeNetwork network, List<long> entryIds)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			if (network == null)
				throw new ArgumentNullException(nameof(network));

			return await RemoveAllowListEntriesAsync(drmAccount.Username, drmAccount.Password, network.Id, entryIds);
		}

		/// <summary>
		/// Removes a list of allow list entries from the given network ID using a <see cref="DRMAccount"/>.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="networkId">The ID of the network to remove entries from.</param>
		/// <param name="entries">The list of allow list entries to remove. Entries must have a server-assigned ID.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account or entry list is null.</exception>
		/// <exception cref="ArgumentException">If the entry list is empty or any entry has a null ID.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<AllowListEntryResult> RemoveAllowListEntriesAsync(DRMAccount drmAccount, long networkId, List<AllowListEntry> entries)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			if (entries == null || entries.Count == 0)
				throw new ArgumentException(ERROR_ENTRY_LIST_EMPTY, nameof(entries));

			if (entries.Any(e => e.Id == null))
				throw new ArgumentException(ERROR_ENTRY_ID_NULL, nameof(entries));

			return await RemoveAllowListEntriesAsync(drmAccount.Username, drmAccount.Password, networkId, entries.Select(e => e.Id.Value).ToList());
		}

		/// <summary>
		/// Removes allow list entries by ID from the given network ID using a <see cref="DRMAccount"/>.
		/// </summary>
		/// <param name="drmAccount">The <see cref="DRMAccount"/> containing authentication credentials.</param>
		/// <param name="networkId">The ID of the network to remove entries from.</param>
		/// <param name="entryIds">The list of allow list entry IDs to remove.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the DRM account or entry ID list is null.</exception>
		/// <exception cref="ArgumentException">If the entry ID list is empty.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<AllowListEntryResult> RemoveAllowListEntriesAsync(DRMAccount drmAccount, long networkId, List<long> entryIds)
		{
			if (drmAccount == null)
				throw new ArgumentNullException(nameof(drmAccount));

			return await RemoveAllowListEntriesAsync(drmAccount.Username, drmAccount.Password, networkId, entryIds);
		}

		/// <summary>
		/// Removes allow list entries by ID from the given network ID using raw credentials.
		/// </summary>
		/// <param name="drmUser">The username for Digi Remote Manager authentication.</param>
		/// <param name="drmPassword">The password for Digi Remote Manager authentication.</param>
		/// <param name="networkId">The ID of the network to remove entries from.</param>
		/// <param name="entryIds">The list of allow list entry IDs to remove.</param>
		/// <returns>A task that returns the result of the operation.</returns>
		/// <exception cref="ArgumentNullException">If the username or password is null.</exception>
		/// <exception cref="ArgumentException">If the entry ID list is null or empty.</exception>
		/// <exception cref="DRMException">If the request fails or the server response is invalid.</exception>
		public static async Task<AllowListEntryResult> RemoveAllowListEntriesAsync(string drmUser, string drmPassword, long networkId, List<long> entryIds)
		{
			if (entryIds == null || entryIds.Count == 0)
				throw new ArgumentException(ERROR_ENTRY_IDS_EMPTY, nameof(entryIds));

			using (HttpClient client = CreateAuthenticatedClient(drmUser, drmPassword))
			{
				try
				{
					string ids = string.Join(",", entryIds);
					string url = $"{ENDPOINT_NETWORKS_INV}/{networkId}/allow_list?ids={ids}";
					HttpResponseMessage response = await client.DeleteAsync(url);
					string responseBody = await response.Content.ReadAsStringAsync();

					if (response.IsSuccessStatusCode)
					{
						return new AllowListEntryResult { IsSuccess = true };
					}
					else
					{
						return new AllowListEntryResult
						{
							IsSuccess = false,
							ErrorMessage = ExtractErrorMessage(responseBody),
							ErrorCode = (int)response.StatusCode
						};
					}
				}
				catch (HttpRequestException ex)
				{
					throw new DRMException(ERROR_NETWORK, null, ex);
				}
				catch (DRMException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new DRMException(ERROR_OTHER, null, ex);
				}
			}
		}

		/// <summary>
		/// Creates an <see cref="HttpClient"/> with Basic authentication configured for Digi Remote Manager.
		/// </summary>
		/// <param name="drmUser">The DRM username.</param>
		/// <param name="drmPassword">The DRM password.</param>
		/// <returns>A configured <see cref="HttpClient"/> instance.</returns>
		/// <exception cref="ArgumentNullException">If the username or password is null or whitespace.</exception>
		private static HttpClient CreateAuthenticatedClient(string drmUser, string drmPassword)
		{
			if (string.IsNullOrWhiteSpace(drmUser))
				throw new ArgumentNullException(nameof(drmUser));
			if (string.IsNullOrWhiteSpace(drmPassword))
				throw new ArgumentNullException(nameof(drmPassword));

			var client = new HttpClient(new SocketsHttpHandler());
			string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{drmUser}:{drmPassword}"));
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
			return client;
		}

		/// <summary>
		/// Attempts to extract a descriptive error message from a JSON response body.
		/// </summary>
		/// <param name="responseBody">The raw response body string.</param>
		/// <returns>The extracted error message, or the original body if extraction fails.</returns>
		private static string ExtractErrorMessage(string responseBody)
		{
			try
			{
				var errorObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
				if (errorObject != null && errorObject.ContainsKey("error_message"))
					return errorObject["error_message"]?.ToString();
				if (errorObject != null && errorObject.ContainsKey("description"))
					return errorObject["description"]?.ToString();
			}
			catch (JsonException) { }

			return responseBody;
		}
	}
}
