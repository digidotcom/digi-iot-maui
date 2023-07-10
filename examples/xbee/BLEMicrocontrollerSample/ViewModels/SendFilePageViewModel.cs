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

using Acr.UserDialogs;
using BLEMicrocontrollerSample.Models;
using BLEMicrocontrollerSample.ViewModels;
using System.Text;
using XBeeLibrary.Core.Events.Relay;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Packet;

namespace BleMicrocontrollerSample.ViewModels
{
	public class SendFilePageViewModel : ViewModelBase
	{
		// Constants.
		private static readonly string MSG_START = "START@@@{0}";
		private static readonly string MSG_END = "END";
		private static readonly string MSG_ACK = "OK";

		private static readonly int BLOCK_SIZE = 128;
		private static readonly int ACK_TIMEOUT = 5000;

		private static readonly string PERCENTAGE_FORMAT = "{0} %";

		// Variables.
		private bool ackReceived = false;
		private bool sendFileButtonEnabled = true;

		private readonly object ackLock = new();

		private double progressLayoutOpacity = 0.2;
		private double progressNumber;

		private string progressText;
		private string fileName = "-";

		// Properties.
		public BleDevice BleDevice { get; private set; }

		public double ProgressLayoutOpacity
		{
			get { return progressLayoutOpacity; }
			set
			{
				progressLayoutOpacity = value;
				RaisePropertyChangedEvent(nameof(ProgressLayoutOpacity));
			}
		}

		public string ProgressText
		{
			get { return progressText; }
			set
			{
				progressText = value;
				RaisePropertyChangedEvent(nameof(ProgressText));
			}
		}

		public double ProgressNumber
		{
			get { return progressNumber; }
			set
			{
				progressNumber = value;
				RaisePropertyChangedEvent(nameof(ProgressNumber));
			}
		}

		public string FileName
		{
			get { return fileName; }
			set
			{
				fileName = value;
				RaisePropertyChangedEvent(nameof(FileName));
			}
		}

		public bool SendFileButtonEnabled
		{
			get { return sendFileButtonEnabled; }
			set
			{
				sendFileButtonEnabled = value;
				RaisePropertyChangedEvent(nameof(SendFileButtonEnabled));
			}
		}

		/// <summary>
		/// Class constructor. Instantiates a new <c>SendFilePageViewModel</c> object with the
		/// given Bluetooth device.
		/// </summary>
		/// <param name="device">Bluetooth device.</param>
		public SendFilePageViewModel(BleDevice device)
		{
			BleDevice = device;
		}

		/// <summary>
		/// Registers the event handler to be notified when new data from the serial interface
		/// is received.
		/// </summary>
		public void RegisterEventHandler()
		{
			BleDevice.XBeeDevice.SerialDataReceived += SerialDataReceived;
		}

		/// <summary>
		/// Unregisters the event handler to be notified when new data from the serial interface is
		/// received.
		/// </summary>
		public void UnregisterEventHandler()
		{
			BleDevice.XBeeDevice.SerialDataReceived -= SerialDataReceived;
		}

		/// <summary>
		/// Shows a file picker to choose the file and sends it to the XBee serial interface.
		/// </summary>
		public async void LoadFile()
		{
			// Show a file picker to choose the file to send.
			FileResult fileData = await FilePicker.PickAsync();
			if (fileData == null)
				return;

			await Task.Run(() =>
			{
				// Pre-configure the UI.
				ProgressLayoutOpacity = 1;
				FileName = fileData.FileName;
				SendFileButtonEnabled = false;

				// Start the send process.
				SendFile(fileData);

				// Post-configure the UI.
				ProgressLayoutOpacity = 0.2;
				SendFileButtonEnabled = true;
			});
		}

		/// <summary>
		/// Sends the file corresponding to the given file data split in blocks to the XBee serial
		/// interface.
		/// </summary>
		/// <param name="fileResult">File result corresponding to the file to send.</param>
		public void SendFile(FileResult fileResult)
		{
			try
			{
				// Send the 'START' message.
				if (!SendDataAndWaitResponse(Encoding.Default.GetBytes(string.Format(MSG_START, fileResult.FileName))))
					return;

				// Get the file bytes.
				var stream = fileResult.OpenReadAsync().Result;
				byte[] fileData = new byte[stream.Length];
				stream.Read(fileData, 0, (int)stream.Length);

				// Split the file in blocks.
				List<byte[]> fileBlocks = GetFileBlocks(fileData);
				for (int i = 0; i < fileBlocks.Count; i++)
				{
					byte[] block = fileBlocks[i];
					using (MemoryStream ms = new())
					{
						using (BinaryWriter bw = new(ms))
						{
							// Append the checksum to the block payload.
							bw.Write(block);
							bw.Write(GetChecksum(block));
						}
						// Send the block.
						if (!SendDataAndWaitResponse(ms.ToArray()))
							return;
						// Update the progress.
						UpdateProgress(100 * (i + 1) / fileBlocks.Count);
					}
				}

				// Send the 'END' message.
				if (!SendDataAndWaitResponse(Encoding.Default.GetBytes(MSG_END)))
					return;

				// Show a message to notify the completion.
				UserDialogs.Instance.Toast("File sent successfully");
			}
			catch (XBeeException e)
			{
				ShowErrorDialog("Could not send the file", e.Message);
			}
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override async void DisconnectDevice()
		{
			await Task.Run(() =>
			{
				// Close the connection.
				BleDevice.XBeeDevice.Disconnect();

				// Go to the root page.
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.Navigation.PopToRootAsync();
				});
			});
		}
            

		/// <summary>
		/// Splits the given file in blocks.
		/// </summary>
		/// <param name="file">File to split in blocks.</param>
		/// <returns>List of blocks.</returns>
		private static List<byte[]> GetFileBlocks(byte[] file)
		{
			List<byte[]> fileBlocks = new();
			int index = 0;
			while (index < file.Length)
			{
				int length = (file.Length - index > BLOCK_SIZE) ? BLOCK_SIZE : (file.Length - index);
				byte[] block = new byte[length];
				Array.Copy(file, index, block, 0, length);
				index += length;
				fileBlocks.Add(block);
			}
			return fileBlocks;
		}

		/// <summary>
		/// Calculates and returns the checksum of the given block.
		/// </summary>
		/// <param name="block">Block to calculate the checksum.</param>
		/// <returns>Checksum of the block.</returns>
		private static byte GetChecksum(byte[] block)
		{
			XBeeChecksum checksum = new();
			checksum.Add(block);
			return checksum.Generate();
		}

		/// <summary>
		/// Updates the progress in the UI.
		/// </summary>
		/// <param name="percentage">Percentage of the send process.</param>
		private void UpdateProgress(int percentage)
		{
			ProgressNumber = percentage / 100.0;
			ProgressText = string.Format(PERCENTAGE_FORMAT, percentage);
		}

		/// <summary>
		/// Serial data received event handler.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			// If the response is "OK", notify the lock to continue the process.
			if (Encoding.Default.GetString(e.Data).Equals(MSG_ACK))
			{
				ackReceived = true;
				lock (ackLock)
				{
					Monitor.Pulse(ackLock);
				}
			}
		}

		/// <summary>
		/// Sends the given data and waits for an ACK response during the configured timeout.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <returns><c>true</c> if the ACK was received, <c>false</c> otherwise.</returns>
		/// <exception cref="XBeeException">If there is any problem sending the data.</exception>
		private bool SendDataAndWaitResponse(byte[] data)
		{
			ackReceived = false;
			// Send the data.
			BleDevice.XBeeDevice.SendSerialData(data);
			// Wait until the ACK is received.
			lock (ackLock)
			{
				Monitor.Wait(ackLock, ACK_TIMEOUT);
			}
			// If the ACK was not received, show an error.
			if (!ackReceived)
			{
				ShowErrorDialog("Error sending file", "Could not send the file, ACK not received.");
				return false;
			}

			return true;
		}
	}
}
