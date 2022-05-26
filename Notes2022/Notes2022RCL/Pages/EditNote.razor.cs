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
// Name: EditNote.razor.cs
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
// <copyright file="EditNote.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    /// <summary>
    /// Class EditNote.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class EditNote
    {
        /// <summary>
        /// Gets or sets the note identifier.
        /// </summary>
        /// <value>The note identifier.</value>
        [Parameter]
        public long NoteId { get; set; } //  what we are editing

        /// <summary>
        /// our data for the note in edit model
        /// </summary>
        /// <value>The model.</value>
        protected TextViewModel Model { get; set; } = new TextViewModel();
        /// <summary>
        /// A note display model
        /// </summary>
        /// <value>The stuff.</value>
        protected DisplayModel stuff { get; set; }

        /// <summary>
        /// The go
        /// </summary>
        protected bool go = false;
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditNote" /> class.
        /// </summary>
        public EditNote()
        {
        }

        // get all the data
        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            stuff = await Client.GetNoteContentAsync(new DisplayModelRequest()
            { NoteId = NoteId, Vers = 0 }, myState.AuthHeader);
            Model.NoteFileID = stuff.NoteFile.Id;
            Model.NoteID = NoteId;
            Model.BaseNoteHeaderID = stuff.Header.BaseNoteId;
            Model.RefId = stuff.Header.RefId;
            Model.MyNote = stuff.Content.NoteBody;
            Model.MySubject = stuff.Header.NoteSubject;
            Model.DirectorMessage = stuff.Header.DirectorMessage;
            string tags = "";
            foreach (var tag in stuff.Tags)
            {
                tags += tag + " ";
            }

            Model.TagLine = tags;
            go = true;
        }
    }
}