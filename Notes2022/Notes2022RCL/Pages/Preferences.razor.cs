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
// Name: Preferences.razor.cs
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
// <copyright file="Preferences.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    /// <summary>
    /// Class Preferences.
    /// Implements the <see cref="Microsoft.AspNetCore.Components.ComponentBase" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
    public partial class Preferences
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the user data.
        /// </summary>
        /// <value>The user data.</value>
        private GAppUser UserData { get; set; }

        /// <summary>
        /// Gets or sets the current text.
        /// </summary>
        /// <value>The current text.</value>
        private string currentText { get; set; }

        /// <summary>
        /// Gets or sets my sizes.
        /// </summary>
        /// <value>My sizes.</value>
        private List<LocalModel2> MySizes { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        private string pageSize { get; set; }

        /// <summary>
        /// Creates new check.
        /// </summary>
        /// <value>The new check.</value>
        private string NewCheck { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// On initialized as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            UserData = await Client.GetUserDataAsync(new NoRequest(), myState.AuthHeader);
            pageSize = UserData.Ipref2.ToString();
            NewCheck = UserData.Ipref0.ToString();
            MySizes = new List<LocalModel2> { new LocalModel2("0", "All"), new LocalModel2("5"), new LocalModel2("10"), new LocalModel2("12"), new LocalModel2("20") };
            currentText = " ";
        }

        /// <summary>
        /// Called when [submit].
        /// </summary>
        private async Task OnSubmit()
        {
            UserData.Ipref2 = int.Parse(pageSize);
            Globals.Interval = UserData.Ipref0 = int.Parse(NewCheck);
            await Client.UpdateUserDataAsync(UserData, myState.AuthHeader);
            Navigation.NavigateTo("");
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            Navigation.NavigateTo("");
        }

        /// <summary>
        /// Class LocalModel2.
        /// </summary>
        public class LocalModel2
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LocalModel2" /> class.
            /// </summary>
            /// <param name="psize">The psize.</param>
            public LocalModel2(string psize)
            {
                Psize = psize;
                Name = psize;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalModel2" /> class.
            /// </summary>
            /// <param name="psize">The psize.</param>
            /// <param name="name">The name.</param>
            public LocalModel2(string psize, string name)
            {
                Psize = psize;
                Name = name;
            }

            /// <summary>
            /// Gets or sets the psize.
            /// </summary>
            /// <value>The psize.</value>
            public string Psize { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }
        }
    }
}