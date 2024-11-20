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

using Newtonsoft.Json;

namespace DigiIoT.Maui.Types
{

    /// <summary>
    /// Represents the response from Digi Remote Manager when provisioning multiple devices.
    /// </summary>
    internal class DeviceProvisionResponse
    {
        /// <summary>
        /// Gets or sets the total number of devices included in the response.
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the list of results for each device in the provisioning operation.
        /// </summary>
        [JsonProperty("list")]
        public List<DeviceResult> List { get; set; }
    }

    /// <summary>
    /// Represents the raw result of a single device provisioning from the API.
    /// </summary>
    internal class DeviceResult
    {
        // Properties.
        /// <summary>
        /// The error status code if the provisioning failed (optional).
        /// </summary>
        [JsonProperty("error_status")]
        public int? ErrorStatus { get; set; }

        /// <summary>
        /// The error message if the provisioning failed (optional).
        /// </summary>
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The context of the error, containing additional details (optional).
        /// </summary>
        [JsonProperty("error_context")]
        public ErrorContext ErrorContext { get; set; }

        /// <summary>
        /// The ID of the device being provisioned.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    /// <summary>
    /// Represents additional context information for a device provisioning error.
    /// </summary>
    internal class ErrorContext
    {
        // Properties.
        /// <summary>
        /// The ID of the device involved in the error.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The install code of the device involved in the error (optional).
        /// </summary>
        [JsonProperty("install_code")]
        public string InstallCode { get; set; }

        /// <summary>
        /// The customer ID associated with the device.
        /// </summary>
        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Whether the device is in a maintenance window or not.
        /// </summary>
        [JsonProperty("in_maintenance_window")]
        public string InMaintenanceWindow { get; set; }

        /// <summary>
        /// Whether the device has an authenticated connection or not.
        /// </summary>
        [JsonProperty("authenticated_connection")]
        public bool AuthenticatedConnection { get; set; }

        /// <summary>
        /// The group associated with the device (optional).
        /// </summary>
        [JsonProperty("group")]
        public string Group { get; set; }
    }
}
