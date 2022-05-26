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
// Name: ShowNote.razor.cs
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
// <copyright file="ShowNote.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    /// <summary>
    /// Class ShowNote.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class ShowNote
    {
        /// <summary>
        /// Gets or sets the note identifier.
        /// </summary>
        /// <value>The note identifier.</value>
        [Parameter]
        public long NoteId { get; set; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>The file identifier.</value>
        public int FileId { get; set; }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject]
        NavigationManager Navigation { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowNote" /> class.
        /// </summary>
        public ShowNote()
        {
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            bool x = myState.IsAuthenticated;
            if (!x)
            {
                await myState.GetLoginReplyAsync();
                if (!myState.IsAuthenticated)
                {
                    Globals.returnUrl = Navigation.Uri;
                    Navigation.NavigateTo("authentication/login");
                }
            }

            // find the file id for this note - get note header
            FileId = (await Client.GetHeaderForNoteIdAsync(new NoteId()
            { Id = NoteId }, myState.AuthHeader)).NoteFileId;
            Globals.GotoNote = NoteId;
            Navigation.NavigateTo("noteindex/" + FileId);
        }
    }
}