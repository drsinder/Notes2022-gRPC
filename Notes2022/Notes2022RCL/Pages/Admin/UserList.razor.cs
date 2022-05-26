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
// Name: UserList.razor.cs
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
// <copyright file="UserList.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Notes2022RCL.Dialogs;

namespace Notes2022RCL.Pages.Admin
{
    /// <summary>
    /// Class UserList.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class UserList
    {
        /// <summary>
        /// Gets or sets the modal.
        /// </summary>
        /// <value>The modal.</value>
        [CascadingParameter]
        public IModalService Modal { get; set; }

        /// <summary>
        /// Gets or sets the u list.
        /// </summary>
        /// <value>The u list.</value>
        private GAppUserList UList { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserList" /> class.
        /// </summary>
        public UserList()
        {
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            UList = await Client.GetUserListAsync(new NoRequest(), myState.AuthHeader);
        }

        /// <summary>
        /// Edits the link.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        protected void EditLink(string Id)
        {
            ModalParameters Parameters = new ModalParameters();
            Parameters.Add("UserId", Id);
            Modal.Show<UserEdit>("", Parameters);
        }
    }
}