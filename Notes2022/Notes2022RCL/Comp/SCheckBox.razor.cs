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
// Name: SCheckBox.razor.cs
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
// <copyright file="SCheckBox.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Notes2022RCL.Pages;

namespace Notes2022RCL.Comp
{
    /// <summary>
    /// Class SCheckBox.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class SCheckBox
    {
        /// <summary>
        /// Gets or sets the tracker.
        /// </summary>
        /// <value>The tracker.</value>
        [Parameter]
        public Tracker Tracker { get; set; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>The file identifier.</value>
        [Parameter]
#pragma warning disable IDE1006 // Naming Styles

        public int fileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        [Parameter]
        public bool isChecked { get; set; }

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public SCheckModel Model { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            Model = new SCheckModel { IsChecked = isChecked, FileId = fileId };
        }

        /// <summary>
        /// Called when [click].
        /// </summary>
        public async Task OnClick()
        {
            isChecked = !isChecked;
            if (isChecked) // create item
            {
                await Client.CreateSequencerAsync(Model, myState.AuthHeader);
            }
            else // delete it
            {
                await Client.DeleteSequencerAsync(Model, myState.AuthHeader);
            }

            await Tracker.Shuffle();
        }
    }
}