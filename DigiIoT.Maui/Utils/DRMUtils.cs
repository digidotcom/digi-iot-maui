/*
 * Copyright 2024,2025, Digi International Inc.
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
		private const string URL_REMOTE_MANAGER = "https://devicecloud.digi.com";
		private const string ENDPOINT_DEVICES_INV = $"{URL_REMOTE_MANAGER}/ws/v1/devices/inventory";

		private static readonly Regex DEVICE_ID_REGEX = new Regex(
			@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{8}-[0-9a-fA-F]{8}-[0-9a-fA-F]{8}$",
			RegexOptions.Compiled);


		private const string ERROR_DEVICE_ID_EMPTY = "Device ID cannot be null or empty.";
		private const string ERROR_DEVICE_ID_INVALID = "Device ID format is not valid. Expected format is 00000000-00000000-00000000-00000000.";
		private const string ERROR_DEVICE_LIST_EMPTY = "Device list cannot be null or empty.";
		private const string ERROR_PROVISIONING_FAILED = "Device provisioning failed. Status Code: {0}, Details: {1}";
		private const string ERROR_NETWORK = "A network error occurred while provisioning the device.";
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
			if (string.IsNullOrWhiteSpace(drmUser))
				throw new ArgumentNullException(nameof(drmUser));

			if (string.IsNullOrWhiteSpace(drmPassword))
				throw new ArgumentNullException(nameof(drmPassword));

			if (devices == null || devices.Count == 0)
				throw new ArgumentException(ERROR_DEVICE_LIST_EMPTY, nameof(devices));

			using (HttpClient client = new HttpClient(new SocketsHttpHandler()))
			{
				try
				{
					string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{drmUser}:{drmPassword}"));
					client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

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
						// Handle other unexpected statuses.
						string errorMessage = await response.Content.ReadAsStringAsync();
						try
						{
							// Attempt to parse the content as JSON and extract "error_message".
							var errorObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(errorMessage);
							if (errorObject != null && errorObject.ContainsKey("error_message"))
								errorMessage = errorObject["error_message"]?.ToString();
							else if (errorObject != null && errorObject.ContainsKey("description"))
								errorMessage = errorObject["description"]?.ToString();
						}
						catch (JsonException ignore) { }

						throw new DRMException(
							string.Format(ERROR_PROVISIONING_FAILED, response.StatusCode, errorMessage),
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
	}
}
