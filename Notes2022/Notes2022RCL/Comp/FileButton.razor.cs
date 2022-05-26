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
// Name: FileButton.razor.cs
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
// <copyright file="FileButton.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Comp
{
    /// <summary>
    /// Class FileButton.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class FileButton
    {
        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        [Parameter]
        public NoteFile NoteFile { get; set; }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject]
        NavigationManager Navigation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileButton" /> class.
        /// </summary>
        public FileButton()
        {
        }

        /// <summary>
        /// Called when [click].
        /// </summary>
        protected void OnClick()
        {
            Navigation.NavigateTo("noteindex/" + NoteFile.Id);
        }
    }
}