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

using Microsoft.Maui.Handlers;
using UIKit;

namespace BLENetworkConfigurationSample.iOS
{
	internal class ContentPageHandler : PageHandler
	{
		// Constants.
		private static readonly string MENU_TITLE = "Options";
		private static readonly string MENU_CANCEL = "Cancel";

		// Variables.
		private List<ToolbarItem> _secondaryItems;

		protected override void ConnectHandler(Microsoft.Maui.Platform.ContentView nativeView)
		{
			base.ConnectHandler(nativeView);
			ContentPage.Loaded += OnLoaded;
		}

		protected override void DisconnectHandler(Microsoft.Maui.Platform.ContentView nativeView)
		{
			ContentPage.Loaded -= OnLoaded;
			base.DisconnectHandler(nativeView);
		}

		ContentPage ContentPage => VirtualView as ContentPage;

		void OnLoaded(object sender, EventArgs e)
		{
			ModifyNavBarMenu();
			DisableSwipeBack();
		}

		/// <summary>
		/// Modifies the navigation bar menu to put secondary toolbar items in an action sheet.
		/// </summary>
		void ModifyNavBarMenu()
		{
			if (this is IPlatformViewHandler handler
					&& handler.ViewController?.ParentViewController?.NavigationItem is UINavigationItem navItem)
			{
				// Get the list of secondary toolbar items.
				List<ToolbarItem> secondaryItemsTemp = ContentPage.ToolbarItems.Where(i => i.Order == ToolbarItemOrder.Secondary).ToList();
				_secondaryItems = new List<ToolbarItem>(secondaryItemsTemp.Count);
				// Clone the items in a new list because elements such as the 'Command'
				// are reset when the item is removed form the page.
				secondaryItemsTemp.ForEach((toolbarItem) =>
				{
					ToolbarItem clonedItem = new()
					{
						Command = toolbarItem.Command,
						Text = toolbarItem.Text,
						CommandParameter = toolbarItem.CommandParameter
					};
					_secondaryItems.Add(clonedItem);
				});
				// Remove the secondary toolbar items from the page.
				secondaryItemsTemp.ForEach(t => ContentPage.ToolbarItems.Remove(t));

				if (_secondaryItems != null && _secondaryItems.Count > 0)
				{
					ContentPage.ToolbarItems.Add(new ToolbarItem()
					{
						Order = ToolbarItemOrder.Primary,
						Priority = 1,
						Text = MENU_TITLE,
						Command = new Command(() =>
						{
							ShowActionsMenu();
						})
					});
					// Just as reference, use 'NavigationController.TopViewController.NavigationItem.RightBarButtonItem'
					// to get the toolbar button that displays the actions sheet.
				}
			}
		}

		/// <summary>
		/// Disables the swipe back gesture from the page.
		/// </summary>
		void DisableSwipeBack()
		{
			if (this is IPlatformViewHandler handler
				&& handler.ViewController?.ParentViewController?.NavigationController is UINavigationController navController)
			{
				navController.InteractivePopGestureRecognizer.Enabled = false;
			}
		}

		/// <summary>
		/// Displays an action sheet with all the actions (secondary items) 
		/// of the current page.
		/// </summary>
		private async void ShowActionsMenu()
		{
			// Generate the list of actions to display.
			string[] menuOptions = new string[_secondaryItems.Count];
			for (int i = 0; i < _secondaryItems.Count; i++)
				menuOptions[i] = _secondaryItems[i].Text;

			// Display the action sheet and get the selected action.
			var action = await ContentPage.DisplayActionSheet(MENU_TITLE, MENU_CANCEL, null, menuOptions);

			// Execute the command corresponding to the selected action.
			foreach (var toolbarItem in _secondaryItems)
			{
				if (toolbarItem.Text.Equals(action))
				{
					toolbarItem.Command.Execute(toolbarItem.CommandParameter);
					return;
				}
			}
		}
	}
}