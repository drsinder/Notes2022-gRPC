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
// Name: NotesFiles.razor.cs
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
// <copyright file="NotesFiles.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Notes2022.Proto;
using Syncfusion.Blazor.Grids;

namespace Notes2022RCL.Pages
{
    /// <summary>
    /// Class NotesFiles.
    /// Implements the <see cref="Microsoft.AspNetCore.Components.ComponentBase" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
    public partial class NotesFiles
    {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        private NoteFileList Files { get; set; }

        /// <summary>
        /// Gets or sets the user data.
        /// </summary>
        /// <value>The user data.</value>
        private GAppUser UserData { get; set; }

        /// <summary>
        /// Set up and get data from server
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            await sessionStorage.SetItemAsync("ArcId", 0);
            await sessionStorage.SetItemAsync("IndexPage", 1);
            // grab data from server
            HomePageModel model = await Client.GetHomePageModelAsync(new NoRequest(), myState.AuthHeader);
            Files = model.NoteFiles;
            UserData = model.UserData;
            if (UserData.Ipref2 == 0)
                UserData.Ipref2 = 10;
        }

        /// <summary>
        /// Displays it.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected void DisplayIt(RowSelectEventArgs<NoteFile> args)
        {
            Navigation.NavigateTo("noteindex/" + args.Data.Id);
        }
    }
}