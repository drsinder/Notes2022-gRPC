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
using Blazored;
using Blazored.Modal;
using Blazored.Modal.Services;
using W8lessLabs.Blazor.LocalFiles;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.LinearGauge;
using Syncfusion.Blazor.SplitButtons;
using Syncfusion.Blazor.Calendars;
using Microsoft.AspNetCore.Components;
using Grpc.Core;
using Grpc.Net.Client.Web;
using Notes2022RCL;
using Notes2022.Proto;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Inputs;
using System.ComponentModel.DataAnnotations;

namespace Notes2022RCL.Pages.Authentication
{
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
        protected InputModel Input = new InputModel{Email = string.Empty, Password = string.Empty};
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
            {Email = Input.Email, Password = Input.Password, Hours = Input.RememberHours};
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