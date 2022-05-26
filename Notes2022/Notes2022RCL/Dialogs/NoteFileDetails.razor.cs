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
// Name: NoteFileDetails.razor.cs
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
// <copyright file="NoteFileDetails.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Blazored.Modal;
using Microsoft.AspNetCore.Components;

namespace Notes2022RCL.Dialogs
{
    /// <summary>
    /// Class NoteFileDetails.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class NoteFileDetails
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
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Parameter]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file title.
        /// </summary>
        /// <value>The file title.</value>
        [Parameter]
        public string FileTitle { get; set; }

        /// <summary>
        /// Gets or sets the last edited.
        /// </summary>
        /// <value>The last edited.</value>
        [Parameter]
        public int NumberArchives { get; set; }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>The owner.</value>
        [Parameter]
        public string Owner { get; set; }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }
    }
}