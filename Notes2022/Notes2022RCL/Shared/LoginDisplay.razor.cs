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
// Name: LoginDisplay.razor.cs
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
// <copyright file="LoginDisplay.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
namespace Notes2022RCL.Shared
{
    /// <summary>
    /// Class LoginDisplay.
    /// Implements the <see cref="Microsoft.AspNetCore.Components.ComponentBase" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
    public partial class LoginDisplay
    {
        /// <summary>
        /// Begins the sign out.
        /// </summary>
        private void BeginSignOut()
        {
            Navigation.NavigateTo("authentication/logout");
        }

        /// <summary>
        /// Gotoes the profile.
        /// </summary>
        private void GotoProfile()
        {
            //Navigation.NavigateTo("authentication/profile");
        }

        /// <summary>
        /// Gotoes the register.
        /// </summary>
        private void GotoRegister()
        {
            Navigation.NavigateTo("authentication/register");
        }

        /// <summary>
        /// Gotoes the login.
        /// </summary>
        private void GotoLogin()
        {
            Navigation.NavigateTo("authentication/login");
        }

        /// <summary>
        /// Gotoes the home.
        /// </summary>
        private void GotoHome()
        {
            Navigation.NavigateTo("");
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            Globals.LoginDisplay = this;
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public void Reload()
        {
            //StateHasChanged();
            try
            {
                this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }
    }
}