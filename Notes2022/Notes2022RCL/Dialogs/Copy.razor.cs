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
// Name: Copy.razor.cs
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
// <copyright file="Copy.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Dialogs
{
    /// <summary>
    /// Class Copy.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class Copy
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        [Parameter]
        public NoteHeader Note { get; set; }

        //[Parameter] public UserData UserData { get; set; }
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        private NoteFileList Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [whole string].
        /// </summary>
        /// <value><c>true</c> if [whole string]; otherwise, <c>false</c>.</value>
        private bool WholeString { get; set; }

        /// <summary>
        /// Gets or sets the selected identifier.
        /// </summary>
        /// <value>The selected identifier.</value>
        private int SelectedId { get; set; } = 0;
        /// <summary>
        /// On initialized as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected async override Task OnInitializedAsync()
        {
            Files = await Client.GetNoteFilesOrderedByNameAsync(new NoRequest(), myState.AuthHeader);
            Files.List.Insert(0, new NoteFile { Id = 0, NoteFileName = "Select a file" });
        }

        /// <summary>
        /// Called when [submit].
        /// </summary>
        protected async Task OnSubmit()
        {
            if (SelectedId == 0)
                return;
            CopyModel cm = new CopyModel();
            cm.FileId = SelectedId;
            cm.Note = Note;
            cm.WholeString = WholeString;
            //cm.UserData = UserData;
            await Client.CopyNoteAsync(cm, myState.AuthHeader);
            await ModalInstance.CloseAsync();
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }
    }
}