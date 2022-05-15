// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-21-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-09-2022
// ***********************************************************************
// <copyright file="Register.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.ComponentModel.DataAnnotations;
using Notes2022.Proto;

namespace Notes2022.Client.Pages.Authentication
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
        protected InputModel Input = new() { Email = string.Empty, UserName = string.Empty, Password = string.Empty, Password2 = string.Empty};
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

            RegisterRequest regreq = new() { Email = Input.Email, Password = Input.Password, Username = Input.UserName };

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