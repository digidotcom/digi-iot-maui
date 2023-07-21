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

using CommunityToolkit.Maui.Views;

namespace BLEMicroPythonSample.Pages;

public partial class PasswordPage : Popup
{
	// Properties.
	/// <summary>
	/// Returns the password entered by the user or <c>null</c> if the
	/// popup was cancelled.
	/// </summary>
	public string Password { get; private set; }

	/// <summary>
	/// Class consturctor. Instantiates a new <c>PasswordPage</c> with
	/// the given argument.
	/// </summary>
	/// <param name="authFailed">Whether the first authentication attempt
	/// failed or not.</param>
	public PasswordPage(bool authFailed)
	{
		InitializeComponent();

		authFailedLabel.IsVisible = authFailed;
	}

	/// <summary>
	/// Focuses the password field.
	/// </summary>
	public void FocusPasswordField(object sender, EventArgs e)
	{
		passwordEntry.Focus();
	}

	/// <summary>
	/// Called when the OK buttons is pressed.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">Event arguments.</param>
	private void OnOkPressed(object sender, EventArgs e)
	{
		Password = passwordEntry.Text;
		Close();
	}

	/// <summary>
	/// Called when the Cancel button is pressed.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">Event arguments.</param>
	private void OnCancelPressed(object sender, EventArgs e)
	{
		Password = null;
		Close();
	}
}