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
// Name: Register.razor.cs
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
// <copyright file="Register.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Notes2022.Proto;
using System.ComponentModel.DataAnnotations;

namespace Notes2022RCL.Pages.Authentication
{
    /// <summary>
    /// Class Register.
    /// Implements the <see cref="Microsoft.AspNetCore.Components.ComponentBase" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
    public partial class Register
    {
        /// <summary>
        /// Class InputModel.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>The email.</value>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the name of the user.
            /// </summary>
            /// <value>The name of the user.</value>
            [Required]
            public string UserName { get; set; }

            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            /// <value>The password.</value>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the password2.
            /// </summary>
            /// <value>The password2.</value>
            [Required]
            [DataType(DataType.Password)]
            public string Password2 { get; set; }
        }

        /// <summary>
        /// The input
        /// </summary>
        protected InputModel Input = new()
        { Email = string.Empty, UserName = string.Empty, Password = string.Empty, Password2 = string.Empty };
        /// <summary>
        /// The message
        /// </summary>
        protected string Message = string.Empty;
        /// <summary>
        /// Gotoes the register.
        /// </summary>
        private async Task GotoRegister()
        {
            if (Input.Password != Input.Password2)
            {
                Message = "Passwords do not match!";
                return;
            }

            RegisterRequest regreq = new()
            { Email = Input.Email, Password = Input.Password, Username = Input.UserName };
            AuthReply ar = await AuthClient.RegisterAsync(regreq);
            if (ar.Status != 200)
            {
                Message = ar.Message;
                return;
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.

            Globals.LoginDisplay.Reload();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            Navigation.NavigateTo("");
        }
    }
}