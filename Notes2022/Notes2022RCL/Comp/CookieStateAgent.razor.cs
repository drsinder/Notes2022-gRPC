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
// Name: CookieStateAgent.razor.cs
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
// <copyright file="CookieStateAgent.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Grpc.Core;
using Microsoft.JSInterop;
using Notes2022.Proto;
using System.Text;
using System.Text.Json;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Blazored.Modal;
using Notes2022RCL.Dialogs;
using Blazored.Modal.Services;

namespace Notes2022RCL.Comp
{
    /// <summary>
    /// Class CookieStateAgent.  Handles login state managment. and serves as a client agent for the MasterHub
    /// Implements the <see cref="Microsoft.AspNetCore.Components.ComponentBase" />
    /// Implements the <see cref="System.IAsyncDisposable" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
    /// <seealso cref="System.IAsyncDisposable" />
    public partial class CookieStateAgent
    {
        [CascadingParameter]
        public IModalService Modal { get; set; }

        /// <summary>
        /// Dealing with login related info
        /// </summary>
        private LoginReply? savedLogin;
        ///// <summary>
        ///// The saved login value used while updating cookies
        ///// </summary>
        //private LoginReply? savedLoginValue; // used while updating cookies
        /// <summary>
        /// The module for calling javascript
        /// </summary>
        private IJSObjectReference? module; // for calling javascript
        /// <summary>
        /// Gets or sets the pinger.
        /// </summary>
        /// <value>The pinger.</value>
        private System.Timers.Timer Pinger { get; set; }

        /// <summary>
        /// Gets or sets the master hub connection.
        /// </summary>
        /// <value>The master hub connection.</value>
        public HubConnection? MasterHubConnection { get; set; }

        /// <summary>
        /// The active users list
        /// </summary>
        private List<ActiveUsers> activeUsers;

        /// <summary>
        /// Gets the active users.
        /// </summary>
        /// <value>The active users.</value>
        public List<ActiveUsers> ActiveUsers
        {
            get { return activeUsers; }
        }

        /// <summary>
        /// The user count
        /// </summary>
        private int userCount;

        /// <summary>
        /// Gets the user count.
        /// </summary>
        /// <value>The user count.</value>
        public int UserCount {  get { return userCount; } }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        async ValueTask IAsyncDisposable.DisposeAsync()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            if (module is not null)
            {
                await module.DisposeAsync();
                module = null;
            }

            // tell hub the user is leaving
            if (savedLogin is not null && savedLogin.Status == 200)
                await MasterHubConnection?.SendAsync("CloseSession", savedLogin.Info.Subject, savedLogin.Info.Displayname);

            if (MasterHubConnection is not null)
            {
                await MasterHubConnection.DisposeAsync();
                MasterHubConnection = null;
            }
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {   // connect to hub
            MasterHubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri("/masterhub")).Build();
           
            // handle talk requests
            MasterHubConnection.On<string, string, string, string>("TalkRequest", async (ToclientId, FromclientId, userName, toName) => 
            {
                // informs user of talk request and asks if they want to accept
                IModalReference? ret = ShowYesNo($"{userName} wants to talk.");
                ModalResult x = await ret.Result;
                if (x.Cancelled)
                {
                    await myState.MasterHubConnection?.SendAsync("TalkRejected", FromclientId, toName);
                    return;
                }
                // tell hub talk was accepted.  it will create a group and initiate talk dialogs.
                await myState.MasterHubConnection?.SendAsync("TalkAccepted", ToclientId, FromclientId, userName, toName);
            });

            // Show the talk dailog
            MasterHubConnection.On<string, string, string, string>("TalkAccepted", async (ToclientId, FromclientId, userName, toName) =>
            {
                ModalParameters? parameters = new ModalParameters();
                parameters.Add("ToclientId", ToclientId);
                parameters.Add("FromclientId", FromclientId);
                parameters.Add("userName", userName);
                parameters.Add("toName", toName);
                parameters.Add("Hub", MasterHubConnection);

                Modal.Show<TalkDialog>("Talk", parameters);
            });

            // tell caller the call was rejected
            MasterHubConnection.On<string>("TalkRejected", (message) =>
            {
                ShowMessage(message);
            });

            // receive active user list and count periodically
            MasterHubConnection.On<int, List<ActiveUsers>>("ReceiveActiveUsers", (count, users) =>
            {
                activeUsers = users;
                userCount = count;
            });

            // starts the hub connection
            await MasterHubConnection.StartAsync();

            Pinger = new(60000); // ping server every 60 seconds to keep it alive
            Pinger.Elapsed += Ping;
            Pinger.Enabled = true;
            Pinger.Start();

            // get the configured login cookie name from the server
            AString cookiename = await Client.GetTextFileAsync(new AString()
            { Val = "CookieName" });
            Globals.Cookie = cookiename.Val;

            // load the cokkie read/write javascript
            if (!Globals.IsMaui)
            {
                // JS injected in .razor file - make sure the cookie.js is loaded
                if (module is null)
                    module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");
            }

            if (myState.IsAuthenticated) // nothing more to do here!
                return;

            try
            {
                await GetLoginReplyAsync(); // try to get a cookie to authenticate
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Initializes the talk.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="userName">Name of the user.</param>
        public async Task InitTalk(string clientId, string userName)
        {
            string? FromId = MasterHubConnection?.ConnectionId;

            await myState.MasterHubConnection?.SendAsync("TalkRequest", clientId, FromId, myState.UserInfo.Displayname, userName);

            ModalParameters? parameters = new ModalParameters();
            parameters.Add("MessageInput", "Requesting talk...");
            parameters.Add("TimeToClose", 1500D);
            Modal.Show<MessageBox>("talk", parameters);
        }

        /// <summary>
        /// Pings the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        protected void Ping(Object source, ElapsedEventArgs e)
        {
            if (Pinger.Interval != 60000)
            {
                Pinger.Interval = 60000;    // ping server every 60 seconds to keep it alive
                Pinger.Enabled = true;
                Pinger.Start();
            }

            // efectively a heart beat for the hub to prevent user list deletion
            if (savedLogin is not null && savedLogin.Status == 200)
                MasterHubConnection?.SendAsync("OpenSession", savedLogin.Info.Subject, savedLogin.Info.Displayname).GetAwaiter();

            _ = Client.NoOpAsync(new NoRequest()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Try to get login cookie
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task GetLoginReplyAsync()
        {
            try
            {
                string? cookie;

#if MAUI
                if (Globals.IsMaui)
                {
                    Notes2022MauiLib.MauiFileActions mf = new Notes2022MauiLib.MauiFileActions();
                    cookie = await mf.ReadFromFile(Globals.Cookie + ".json");
                }
                else
#endif
                {
                    if (module is null)
                        module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");

                    cookie = await ReadCookie(Globals.Cookie);
                }

                if (!string.IsNullOrEmpty(cookie))
                {
                    // found a cookie!
                    savedLogin = JsonSerializer.Deserialize<LoginReply>(cookie);

                    // Login at server
                    await Client.ReLoginAsync(new NoRequest(), myState.AuthHeader);
                    
                    if (Globals.NavMenu != null)
                    {
                        await Globals.NavMenu.Reload();
                    }

                    if (Globals.LoginDisplay != null)
                    {
                        Globals.LoginDisplay.Reload();
                    }

                    NotifyStateChanged(); // notify subscribers

                    Pinger.Interval = 500;
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Read a cookie
        /// </summary>
        /// <param name="cookieName">cookie name</param>
        /// <returns>needs to be deserialized)</returns>
        public async Task<string?> ReadCookie(string cookieName)
        {
            if (Globals.IsMaui)
                return null;

            if (module is not null)
            {
                try
                {
                    return Globals.Base64Decode(await module.InvokeAsync<string>("ReadCookie", cookieName));
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Write a Cookie
        /// </summary>
        /// <param name="cookieName">Name of the cookie</param>
        /// <param name="newCookie">Serialized cookie</param>
        /// <param name="hours">expiry</param>
        public async Task WriteCookie(string cookieName, string newCookie, int hours)
        {
            if (Globals.IsMaui)
                return;

            if (module is not null)
            {
                try
                {
                    string stuff = Globals.Base64Encode(newCookie);
                    _ = await module.InvokeAsync<string>("CreateCookie", cookieName, Globals.Base64Encode(newCookie), hours);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Gets or sets the login reply.  Setting also notifies subsrcibers
        /// </summary>
        /// <value>The LoginReply - the current state of login.</value>
        public LoginReply? LoginReply
        {
            get
            {
                return savedLogin;
            }

            set
            {
                if (savedLogin is not null && savedLogin.Status == 200 && (value is null || value.Status != 200))
                    MasterHubConnection?.SendAsync("CloseSession", savedLogin.Info.Subject, savedLogin.Info.Displayname).GetAwaiter();
                else if (value is not null && value.Status == 200)
                    MasterHubConnection?.SendAsync("OpenSession", value.Info.Subject, value.Info.Displayname).GetAwaiter();

                savedLogin = value;

#if MAUI
                if (Globals.IsMaui)
                {
                    Notes2022MauiLib.MauiFileActions mf = new Notes2022MauiLib.MauiFileActions();
                    string filename = Globals.Cookie + ".json";
                    if (savedLogin is not null)
                    {
                        _ = mf.SaveToFileAndClipBoard(filename, Encoding.ASCII.GetBytes( JsonSerializer.Serialize(savedLogin)), false).GetAwaiter().GetResult();
                    }
                    else
                    {
                        mf.DeleteFile(filename);
                    }
                    NotifyStateChanged(); // notify subscribers
                    return;
                }
#endif
                NotifyStateChanged(); // notify subscribers
            }
        }

        /// <summary>
        /// Occurs when Login state changes.
        /// </summary>
        public event System.Action? OnChange;
        /// <summary>
        /// Notifies subscribers of login state change.
        /// </summary>
        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }

        /// <summary>
        /// Check if user is authenticated - Login reply is not null and status == 200
        /// </summary>
        /// <value><c>true</c> if this instance is authenticated; otherwise, <c>false</c>.</value>
        public bool IsAuthenticated
        {
            get
            {
                return (LoginReply is not null) && LoginReply.Status == 200;
            }
        }

        /// <summary>
        /// Is user in Admin role
        /// </summary>
        /// <value><c>true</c> if this instance is admin; otherwise, <c>false</c>.</value>
        public bool IsAdmin
        {
            get
            {
                if (LoginReply is null || LoginReply.Status != 200)
                    return false;
                return UserInfo is not null && UserInfo.IsAdmin;
            }
        }

        /// <summary>
        /// Is user in User role
        /// </summary>
        /// <value><c>true</c> if this instance is user; otherwise, <c>false</c>.</value>
        public bool IsUser
        {
            get
            {
                if (LoginReply is null || LoginReply.Status != 200)
                    return false;
                return UserInfo is not null && UserInfo.IsUser;
            }
        }

        /// <summary>
        /// Get a Metadata/header for authetication to server in gRPC calls
        /// </summary>
        /// <value>The authentication header.</value>
        public Metadata AuthHeader
        {
            get
            {
                Metadata? headers = new();
                if (LoginReply is not null && LoginReply.Status == 200)
                    headers.Add("Authorization", $"Bearer {LoginReply.Jwt}");
                return headers;
            }
        }

        /// <summary>
        /// Get the decoded user info
        /// </summary>
        /// <value>The user information.</value>
        public UserInfo? UserInfo
        {
            get
            {
                if (LoginReply is not null && LoginReply.Status == 200)
                {
                    return LoginReply.Info;
                }

                return null;
            }
        }

        private DateTime lastUpdate { get; set; }

        TimeSpan minUpdate = TimeSpan.FromMilliseconds(400);

        /// <summary>
        /// Handle state change.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ShowMessage(string message)
        {
            if (DateTime.Now - lastUpdate < minUpdate)
                return;

            lastUpdate = DateTime.Now;

            ModalParameters? parameters = new ModalParameters();
            parameters.Add("MessageInput", message);
            Modal.Show<MessageBox>("", parameters);
        }

        private IModalReference? ShowYesNo(string message)
        {
            ModalParameters? parameters = new ModalParameters();
            parameters.Add("MessageInput", message);
            IModalReference? retval = Modal.Show<YesNo>("", parameters);
            return retval;
        }
    }
}