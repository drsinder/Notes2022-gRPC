// ***********************************************************************
// Assembly         : Notes2022RCL
// Author           : Dale Sinder
// Created          : 05-24-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-25-2022
//
// Copyright © 2022, Dale Sinder
//
// Name: NavMenu.razor.cs
//
// Description:
//      TODO
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 3 as
// published by the Free Software Foundation.   
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License version 3 for more details.
//
//  You should have received a copy of the GNU General Public License
//  version 3 along with this program in file "license-gpl-3.0.txt".
//  If not, see<http://www.gnu.org/licenses/gpl-3.0.txt>.
// ***********************************************************************
// <copyright file="NavMenu.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Notes2022.Proto;
using Notes2022RCL.Dialogs;
using Syncfusion.Blazor.Navigations;
using System.Timers;
using MenuItem = Syncfusion.Blazor.Navigations.MenuItem;

namespace Notes2022RCL.Menus
{
    /// <summary>
    /// Class NavMenu.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class NavMenu
    {
        /// <summary>
        /// For display of error message during initialization
        /// </summary>
        /// <value>The modal.</value>
        [CascadingParameter]
        public IModalService Modal { get; set; }

        /// <summary>
        /// The list of menu bar items (structure of the menu)
        /// </summary>
        /// <value>The menu items top.</value>
        protected static List<MenuItem>? menuItemsTop { get; set; }

        /// <summary>
        /// Root menu item
        /// </summary>
        /// <value>The top menu.</value>
        protected SfMenu<MenuItem> topMenu { get; set; }

        /// <summary>
        /// Current time
        /// </summary>
        /// <value>The mytime.</value>
        private string mytime { get; set; }

        /// <summary>
        /// Used to compare time and abort re-render in same minute
        /// </summary>
        /// <value>The mytime2.</value>
        private string mytime2 { get; set; } = "";
        /// <summary>
        /// Used to update menu bar time - tick once per second
        /// </summary>
        /// <value>The timer2.</value>
        private System.Timers.Timer timer2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [do seq].
        /// </summary>
        /// <value><c>true</c> if [do seq]; otherwise, <c>false</c>.</value>
        private bool DoSeq { get; set; } = false;
        /// <summary>
        /// The collapse nav menu
        /// </summary>
        private bool collapseNavMenu = true;
        /// <summary>
        /// Gets the nav menu CSS class.
        /// </summary>
        /// <value>The nav menu CSS class.</value>
        private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;


        private string myHang { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject]
        NavigationManager Navigation { get; set; }

        /// <summary>
        /// Gets or sets the session storage.
        /// </summary>
        /// <value>The session storage.</value>
        [Inject]
        Blazored.SessionStorage.ISessionStorageService sessionStorage { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Initializes a new instance of the <see cref="NavMenu" /> class.
        /// </summary>
        [Inject] IJSRuntime JS { get; set; }
        public NavMenu()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Toggles the nav menu.
        /// </summary>
        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        /// <summary>
        /// Update the clock once per second
        /// </summary>
        /// <param name="firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                timer2 = new System.Timers.Timer(1000);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

                timer2.Elapsed += TimerTick2;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

                timer2.Enabled = true;
                myState.OnChange += StateHasChanged;
            }
        }

        /// <summary>
        /// Invoked once per second
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs" /> instance containing the event data.</param>
        protected void TimerTick2(object source, ElapsedEventArgs e)
        {
            //mytime = DateTime.Now.ToShortTimeString();
            //if (mytime != mytime2) // do we need to re-render?
            //{
            //    StateHasChanged();
            //    mytime2 = mytime;
            //}
            if (DoSeq)
            {
                DoSeq = false;
                StartSeq().GetAwaiter();
            }
        }

        /// <summary>
        /// Invoked when an Item is selected
        /// </summary>
        /// <param name="e">The e.</param>
        public async Task OnSelect(MenuEventArgs<MenuItem> e)
        {
            await ExecMenu(e.Item.Id);
        }

        /// <summary>
        /// This could potentially be called from other places...
        /// </summary>
        /// <param name="id">The identifier.</param>

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        private async Task ExecMenu(string id)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        {
            try
            {
                switch (id)
                {
                    case "MainHelp":
                        Navigation.NavigateTo("help");
                        break;
                    case "About":
                        Navigation.NavigateTo("about");
                        break;
                    case "License":
                        Navigation.NavigateTo("license");
                        break;
                    //case "Subscriptions":
                    //    Navigation.NavigateTo("subscribe");
                    //    break;
                    case "MRecent":
                        Navigation.NavigateTo("tracker");
                        break;
                    case "Recent":
                        DoSeq = true;
                        break;
                    case "NoteFiles":
                        Navigation.NavigateTo("admin/notefilelist");
                        break;
                    case "Preferences":
                        Navigation.NavigateTo("preferences");
                        break;
                    case "Hangfire":
                        await JS.InvokeAsync<object>("open", myState.UserInfo.Hangfire, "_blank");

                        //Navigation.NavigateTo(myState.UserInfo.Hangfire, true);
                        break;
                    case "Roles":
                        Navigation.NavigateTo("admin/editroles");
                        break;
                    //case "Linked":
                    //    Navigation.NavigateTo("admin/linkindex");
                    //    break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public async Task Reload()
        {
            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
            await UpdateMenu();
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            Globals.NavMenu = this;
            await UpdateMenu();
        }

        /// <summary>
        /// Enable only items available to logged in user
        /// </summary>

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.", Justification = "<Pending>")]
        public async Task UpdateMenu()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        {
            try
            {
                bool isAdmin = false;
                bool isUser = false;
                if (myState.IsAuthenticated)
                {
                    isAdmin = myState.IsAdmin;
                    isUser = myState.IsUser;
                }

                // make the whole menu
                menuItemsTop = new List<MenuItem>();
                MenuItem item;
                item = new() { Id = "Recent", Text = "Recent Notes" };
                menuItemsTop.Add(item);

                MenuItem item3 = new()
                { Id = "Manage", Text = "Manage" };
                item3.Items = new List<MenuItem>
                {
                    new() {Id = "MRecent", Text = "Recent"}, //new () { Id = "Subscriptions", Text = "Subscriptions" },
                    new() {Id = "Preferences", Text = "Preferences"}
                };
                menuItemsTop.Add(item3);
                item = new()
                { Id = "Help", Text = "Help" };
                item.Items = new List<MenuItem>
                {
                    new() {Id = "MainHelp", Text = "Help"},
                    new() {Id = "About", Text = "About"}, 
                    new() {Id = "License", Text = "License"}
                };
                menuItemsTop.Add(item);
                MenuItem item4 = new MenuItem()
                { Id = "Admin", Text = "Admin" };
                item4.Items = new List<MenuItem>
                {
                    new() {Id = "NoteFiles", Text = "NoteFiles"},
                    new() {Id = "Roles", Text = "Roles"},
                    new() {Id = "Hangfire", Text = "Jobs-Dashboard"}
                    //new() {Id = "Roles", Text = "Roles"}
                };

                if (Globals.IsMaui)
                    item.Items.RemoveAt(item.Items.Count - 1);

                menuItemsTop.Add(item4);
                // remove what does not apply to this user
                if (!isAdmin)
                {
                    menuItemsTop.RemoveAt(3);
                }

                if (isUser || isAdmin)
                {
                }
                else
                {
                    menuItemsTop.RemoveAt(1);
                    menuItemsTop.RemoveAt(0);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Recent menu item - start sequencing
        /// </summary>
        private async Task StartSeq()
        {
            // get users list of files
            //List<Sequencer> sequencers = await DAL.GetSequencer(Http);
            List<Sequencer> sequencers = (await Client.GetSequencerAsync(new NoRequest(), myState.AuthHeader)).List.ToList();
            if (sequencers.Count == 0)
                return;
            // order them as prefered by user
            sequencers = sequencers.OrderBy(p => p.Ordinal).ToList();
            // set up state for sequencing
            await sessionStorage.SetItemAsync<List<Sequencer>>("SeqList", sequencers);
            await sessionStorage.SetItemAsync<int>("SeqIndex", 0);
            await sessionStorage.SetItemAsync<Sequencer>("SeqItem", sequencers[0]);
            await sessionStorage.SetItemAsync<bool>("IsSeq", true); // flag for noteindex
            // begin
            string go = "noteindex/" + sequencers[0].NoteFileId;
            Navigation.NavigateTo(go);
            return;
        }

        /// <summary>
        /// Show error message
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowMessage(string message)
        {
            var parameters = new ModalParameters();
            parameters.Add("MessageInput", message);
            Modal.Show<MessageBox>("Error", parameters);
        }
    }
}