// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 11-15-2021
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-08-2022
// ***********************************************************************
// <copyright file="LoginDisplay.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;

namespace Notes2022.Client.Shared
{
    /// <summary>
    /// Class LoginDisplay.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
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
            StateHasChanged();
        }
    }
}