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
// Name: AccessList.razor.cs
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
// <copyright file="AccessList.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Syncfusion.Blazor.Grids;

namespace Notes2022RCL.Dialogs
{
    /// <summary>
    /// Class AccessList.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class AccessList
    {
        /// <summary>
        /// Gets or sets the modal.
        /// </summary>
        /// <value>The modal.</value>
        [CascadingParameter]
        public IModalService Modal { get; set; }

        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// File Id we are working on
        /// </summary>
        /// <value>The file identifier.</value>

#pragma warning disable IDE1006 // Naming Styles

        [Parameter]
        public int fileId { get; set; }

        /// <summary>
        /// Grid of tokens
        /// </summary>
        private SfGrid<NoteAccess> MyGrid;
        /// <summary>
        /// List of tokens
        /// </summary>
        /// <value>My list.</value>
        private List<NoteAccess> myList { get; set; }

        /// <summary>
        /// Temp list of tokens
        /// </summary>
        /// <value>The user list.</value>
        private List<GAppUser> userList { get; set; }

        /// <summary>
        /// My access
        /// </summary>
        /// <value>My access.</value>
        private NoteAccess myAccess { get; set; }

        /// <summary>
        /// Gets or sets the arc identifier.
        /// </summary>
        /// <value>The arc identifier.</value>
        private int arcId { get; set; }

        /// <summary>
        /// message to display
        /// </summary>
        /// <value>The message.</value>
        private string message { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessList" /> class.
        /// </summary>
        public AccessList()
        {
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected async override Task OnParametersSetAsync()
        {
            arcId = Globals.ArcId;
            AccessAndUserList myLists = await Client.GetAccessAndUserListAsync(new AccessAndUserListRequest()
            { FileId = fileId, ArcId = arcId, UserId = myState.UserInfo?.Subject }, myState.AuthHeader);
            myList = myLists.AccessList.ToList();
            userList = myLists.AppUsers.List.ToList();
            myAccess = myLists.UserAccess;
        }

        /// <summary>
        /// We are done
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Add a new token for another user
        /// </summary>
        protected async void CreateNew()
        {
            ModalParameters? parameters = new ModalParameters();
            parameters.Add("userList", userList);
            parameters.Add("NoteFileId", fileId);
            IModalReference? xx = Modal.Show<AddAccessDlg>("", parameters);
            await xx.Result;
            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
            await MyGrid.Refresh();
        }

        /// <summary>
        /// Item deleted - refresh list
        /// </summary>
        /// <param name="newMessage">The new message.</param>
        protected async Task ClickHandler(string newMessage)
        {
            arcId = Globals.ArcId;
            NoteAccessList myLists = await Client.GetAccessListAsync(new AccessAndUserListRequest()
            { FileId = fileId, ArcId = arcId, UserId = myState.UserInfo?.Subject }, myState.AuthHeader);
            myList = myLists.List.ToList();
            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
            await MyGrid.Refresh();
        }
    }
}