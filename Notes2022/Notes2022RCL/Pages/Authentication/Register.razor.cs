using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Notes2022RCL;
using Notes2022.Proto;
using Blazored;
using Blazored.Modal;
using Blazored.Modal.Services;
using W8lessLabs.Blazor.LocalFiles;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.LinearGauge;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.SplitButtons;
using Syncfusion.Blazor.Calendars;
using Microsoft.AspNetCore.Components;
using Grpc.Core;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;

namespace Notes2022RCL.Pages.Authentication
{
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
        {Email = string.Empty, UserName = string.Empty, Password = string.Empty, Password2 = string.Empty};
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
            {Email = Input.Email, Password = Input.Password, Username = Input.UserName};
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