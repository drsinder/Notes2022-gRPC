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
// Name: Login.razor.cs
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
// <copyright file="Login.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using System.ComponentModel.DataAnnotations;

namespace Notes2022RCL.Pages.Authentication
{
/// <summary>
/// Class Login.
/// Implements the <see cref="ComponentBase" />
/// </summary>
/// <seealso cref="ComponentBase" />
    public partial class Login
    {
        /// <summary>
        /// Class InputModel.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            /// directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// <value>The email.</value>
            [Required]
            //[EmailAddress]
            public string Email { get; set; }

            /// <summary>
            /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            /// directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// <value>The password.</value>
            //[Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the remember hours.
            /// </summary>
            /// <value>The remember hours.</value>
            public int RememberHours { get; set; }
        }

        /// <summary>
        /// Gets or sets the return URL.
        /// </summary>
        /// <value>The return URL.</value>
        [Parameter]
        public string returnURL { get; set; }

        /// <summary>
        /// Gets or sets my cookie value.
        /// </summary>
        /// <value>My cookie value.</value>
        public string myCookieValue { get; set; } = "";
        /// <summary>
        /// The input
        /// </summary>
        protected InputModel Input = new InputModel { Email = string.Empty, Password = string.Empty };
        /// <summary>
        /// The message
        /// </summary>
        protected string Message = string.Empty;
        /// <summary>
        /// Gotoes the login.
        /// </summary>
        private async Task GotoLogin()
        {
            string retUrl = Globals.returnUrl;
            Globals.returnUrl = string.Empty;
            LoginRequest req = new LoginRequest()
            { Email = Input.Email, Password = Input.Password, Hours = Input.RememberHours };
            LoginReply ar = await AuthClient.LoginAsync(req);
            if (ar.Status == 200)
            {
                ar.Hours = Input.RememberHours;
                myState.LoginReply = ar;
            }
            else
            {
                Message = ar.Message;
                return;
            }

            //await AuthClient.SendEmailAsync(new Email() { Address = Input.Email, Subject = "Login", Body = "Notes 2022 Login" });
            Globals.LoginDisplay?.Reload();
            Globals.NavMenu?.Reload();
            Navigation.NavigateTo(retUrl);
        }

        private async Task GotoResend()
        {
            AuthReply reply = await AuthClient.ResendEmailAsync(new AString() { Val = Input.Email });
            Message = reply.Message;

            StateHasChanged();
        }

        private async Task GotoPassword()
        {
            AuthReply reply = await AuthClient.ResetPasswordAsync(new AString() { Val = Input.Email });
            Message = reply.Message;

            StateHasChanged();
        }
    }
}