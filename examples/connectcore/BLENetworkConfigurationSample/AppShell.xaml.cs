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

using BLENetworkConfigurationSample.ViewModels;

namespace BLENetworkConfigurationSample
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		protected override bool OnBackButtonPressed()
		{
			Dispatcher.Dispatch(async () =>
			{
				if (Current.CurrentPage.BindingContext is ViewModelBase)
				{
					await ((ViewModelBase)Current.CurrentPage.BindingContext).NavigateBack();
				}
			});
			return true;
		}
	}
}