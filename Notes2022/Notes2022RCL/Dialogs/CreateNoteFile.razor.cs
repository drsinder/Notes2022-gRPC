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
// Name: CreateNoteFile.razor.cs
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
// <copyright file="CreateNoteFile.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using System.ComponentModel.DataAnnotations;

namespace Notes2022RCL.Dialogs
{
    /// <summary>
    /// Class CreateNoteFile.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class CreateNoteFile
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>The file identifier.</value>
        [Parameter]
        public int FileId { get; set; }

        /// <summary>
        /// The dummy file
        /// </summary>
        public CreateFileModel dummyFile = new CreateFileModel();
        /// <summary>
        /// Handles the valid submit.
        /// </summary>
        private async Task HandleValidSubmit()
        {
            await Client.CreateNoteFileAsync(new NoteFile()
            { NoteFileName = dummyFile.NoteFileName, NoteFileTitle = dummyFile.NoteFileTitle }, myState.AuthHeader);
            await ModalInstance.CloseAsync();
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Class CreateFileModel.
        /// </summary>
        public class CreateFileModel
        {
            /// <summary>
            /// Gets or sets the name of the note file.
            /// </summary>
            /// <value>The name of the note file.</value>
            [Required]
            public string NoteFileName { get; set; }

            /// <summary>
            /// Gets or sets the note file title.
            /// </summary>
            /// <value>The note file title.</value>
            [Required]
            public string NoteFileTitle { get; set; }
        }
    }
}