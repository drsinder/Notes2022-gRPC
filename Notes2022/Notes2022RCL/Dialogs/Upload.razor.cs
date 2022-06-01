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
// Name: Upload.razor.cs
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
// <copyright file="Upload.razor.cs" company="Notes2022RCL">
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
    /// Class Upload.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class Upload
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        [Parameter]
        public string NoteFile { get; set; }

        /// <summary>
        /// Gets or sets the upload data.
        /// </summary>
        /// <value>The upload file.</value>
        [Parameter]
        public string UploadText { get; set; }

        [Parameter]
        public string JsonFileName { get; set; }


        /// <summary>
        /// On after render as an asynchronous operation.
        /// </summary>
        /// <param name="firstRender">if set to <c>true</c> [first render].</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (string.IsNullOrEmpty(JsonFileName))
                    JsonFileName = "Clipboard";

                var request = new ImportRequest()
                { NoteFile = NoteFile, Payload = UploadText, JsonFileName = JsonFileName };
                _ = await Client.ImportJsonAsync(request, myState.AuthHeader, deadline: DateTime.UtcNow.AddMinutes(10));
                await ModalInstance.CancelAsync();
            }
        }
    }
}