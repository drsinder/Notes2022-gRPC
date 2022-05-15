// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-21-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-06-2022
// ***********************************************************************
// <copyright file="Login.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using System.ComponentModel.DataAnnotations;

namespace Notes2022.Client.Pages.Authentication
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
            [Required]
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
    }
}