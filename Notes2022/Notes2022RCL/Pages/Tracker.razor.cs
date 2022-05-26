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
// Name: Tracker.razor.cs
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
// <copyright file="Tracker.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    /// <summary>
    /// Class Tracker.
    /// Implements the <see cref="Microsoft.AspNetCore.Components.ComponentBase" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
    public partial class Tracker
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the stuff.
        /// </summary>
        /// <value>The stuff.</value>
        private List<NoteFile> stuff { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        private List<NoteFile> files { get; set; }

        /// <summary>
        /// Gets or sets the trackers.
        /// </summary>
        /// <value>The trackers.</value>
        private List<Sequencer> trackers { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            trackers = (await Client.GetSequencerAsync(new NoRequest(), myState.AuthHeader)).List.ToList();
            HomePageModel model = await Client.GetHomePageModelAsync(new NoRequest(), myState.AuthHeader);
            stuff = model.NoteFiles.List.OrderBy(p => p.NoteFileName).ToList();
            await Shuffle();
        }

        /// <summary>
        /// Shuffles this instance.
        /// </summary>
        public async Task Shuffle()
        {
            files = new List<NoteFile>();
            trackers = (await Client.GetSequencerAsync(new NoRequest(), myState.AuthHeader)).List.ToList();
            if (trackers is not null)
            {
                trackers = trackers.OrderBy(p => p.Ordinal).ToList();
                foreach (var tracker in trackers)
                {
#pragma warning disable CS8604 // Possible null reference argument.

                    files.Add(stuff.Find(p => p.Id == tracker.NoteFileId));
#pragma warning restore CS8604 // Possible null reference argument.

                }
            }

            if (stuff is not null && stuff.Count > 0)
            {
                foreach (var s in stuff)
                {
                    if (files.Find(p => p.Id == s.Id) is null)
                        files.Add(s);
                }
            }

            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            NavMan.NavigateTo("");
        }
    }
}