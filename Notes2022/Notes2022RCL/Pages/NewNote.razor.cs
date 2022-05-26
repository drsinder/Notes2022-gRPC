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
// Name: NewNote.razor.cs
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
// <copyright file="NewNote.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    /// <summary>
    /// Class NewNote.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class NewNote
    {
        /// <summary>
        /// Gets or sets the notesfile identifier.
        /// </summary>
        /// <value>The notesfile identifier.</value>
        [Parameter]
        public int NotesfileId { get; set; }

        /// <summary>
        /// Gets or sets the base note header identifier.
        /// </summary>
        /// <value>The base note header identifier.</value>
        [Parameter]
        public long BaseNoteHeaderId { get; set; } //  base note we are responding to

        /// <summary>
        /// Gets or sets the reference identifier.
        /// </summary>
        /// <value>The reference identifier.</value>
        [Parameter]
        public long RefId { get; set; } //  what we are responding to

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        protected TextViewModel Model { get; set; } = new TextViewModel();
        /// <summary>
        /// Gets or sets the HTTP.
        /// </summary>
        /// <value>The HTTP.</value>
        [Inject]
        HttpClient Http { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Initializes a new instance of the <see cref="NewNote" /> class.
        /// </summary>
        public NewNote()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        {
        }

        /// <summary>
        /// Just fill in a few fields and we are ready...
        /// </summary>
        protected override void OnParametersSet()
        {
            Model.NoteFileID = NotesfileId; // which file?
            Model.NoteID = 0; // 0 for new note
            Model.BaseNoteHeaderID = BaseNoteHeaderId; // base note we are responding to
            Model.RefId = RefId; // note we are responding to
            Model.MyNote = "";
            Model.MySubject = "";
            Model.TagLine = "";
            Model.DirectorMessage = "";
        }
    }
}