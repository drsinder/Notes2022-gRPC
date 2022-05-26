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
// Name: About2.razor.cs
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
// <copyright file="About2.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    /// <summary>
    /// Class About2.
    /// Implements the <see cref="Microsoft.AspNetCore.Components.ComponentBase" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
    public partial class About2
    {
#pragma warning disable IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        private AboutModel model { get; set; }

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets up time.
        /// </summary>
        /// <value>Up time.</value>
        private TimeSpan upTime { get; set; }

        /// <summary>
        /// The text
        /// </summary>
        private string text = string.Empty;
        /// <summary>
        /// Get some simple stuff from server
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            text = (await Client.GetTextFileAsync(new AString()
            { Val = "about.html" })).Val;
            try
            {
                model = await Client.GetAboutAsync(new NoRequest());
            }
            finally
            {
            }
        }
    }
}