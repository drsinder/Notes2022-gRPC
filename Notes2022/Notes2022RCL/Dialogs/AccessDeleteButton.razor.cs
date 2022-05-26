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
// Name: AccessDeleteButton.razor.cs
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
// <copyright file="AccessDeleteButton.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Dialogs
{
    /// <summary>
    /// Class AccessDeleteButton.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class AccessDeleteButton
    {
        /// <summary>
        /// Gets or sets the note access.
        /// </summary>
        /// <value>The note access.</value>
        [Parameter]
        public NoteAccess noteAccess { get; set; }

        /// <summary>
        /// Gets or sets the on click.
        /// </summary>
        /// <value>The on click.</value>
        [Parameter]
        public EventCallback<string> OnClick { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessDeleteButton" /> class.
        /// </summary>
        public AccessDeleteButton()
        {
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        protected async Task Delete()
        {
            await Client.DeleteAccessItemAsync(noteAccess, myState.AuthHeader);
            await OnClick.InvokeAsync("Delete");
        }
    }
}