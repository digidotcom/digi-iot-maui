﻿/*
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
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace BLEMicrocontrollerSample.ViewModels
{
	public abstract partial class ViewModelBase : INotifyPropertyChanged
	{
        // Constants.
        public const string BUTTON_OK = "OK";

        // Events.
        public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises a property changed event for the given property name.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void RaisePropertyChangedEvent(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChangedEventArgs e = new(propertyName);
				PropertyChanged(this, e);
			}
		}

		/// <summary>
		/// Shows a loading dialog with the given text.
		/// </summary>
		/// <param name="text">Text to display.</param>
		protected static void ShowLoadingDialog(string text)
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				UserDialogs.Instance.ShowLoading(text);
			});
		}

		/// <summary>
		/// Hides the loading dialog.
		/// </summary>
		protected static void HideLoadingDialog()
		{
            MainThread.BeginInvokeOnMainThread(UserDialogs.Instance.HideLoading);
		}

		/// <summary>
		/// Shows an error dialog with the given title and message.
		/// </summary>
		/// <param name="title">Error title.</param>
		/// <param name="message">Error message.</param>
		protected static void ShowErrorDialog(string title, string message)
		{
            MainThread.BeginInvokeOnMainThread(() =>
			{
				UserDialogs.Instance.Alert(message, title);
			});
		}

        /// <summary>
        /// Displays an alert with the given title and message.
        /// </summary>
        /// <param name="alertTitle">Title of the alert to display.</param>
        /// <param name="alertMessage">Message of the alert to display.</param>
        protected static void DisplayAlert(string alertTitle, string alertMessage)
        {
            DisplayAlert(alertTitle, alertMessage, BUTTON_OK);
        }

        /// <summary>
        /// Displays an alert with the given title and message.
        /// </summary>
        /// <param name="alertTitle">Title of the alert to display.</param>
        /// <param name="alertMessage">Message of the alert to display.</param>
        /// <param name="buttonText">Text to display in the button of the alert dialog.</param>
        protected static void DisplayAlert(string alertTitle, string alertMessage, string buttonText)
        {
            Page currentPage = GetCurrentPage();
            if (currentPage == null)
                return;

            MainThread.InvokeOnMainThreadAsync(() => {
                currentPage.DisplayAlert(alertTitle, alertMessage, buttonText);
            });
        }

        /// <summary>
        /// Returns the current page being displayed.
        /// </summary>
        /// <returns>The current <c>Page</c> being displayed.</returns>
        protected static Page GetCurrentPage()
        {
			return Shell.Current.CurrentPage;
        }

        /// <summary>
        /// Disconnects the device and navigates back.
        /// </summary>
        [RelayCommand]
        public async Task NavigateBack()
        {
            // Ask the user if wants to close the connection.
            if (await GetCurrentPage().DisplayAlert("Disconnect device", "Do you want to disconnect the irrigation device?", "Yes", "No"))
            {
                DisconnectDevice();
                await Shell.Current.GoToAsync("..");
            }
        }

        /// <summary>
        /// Closes the connection with the device and goes to the previous page.
        /// </summary>
        public abstract void DisconnectDevice();
    }
}
