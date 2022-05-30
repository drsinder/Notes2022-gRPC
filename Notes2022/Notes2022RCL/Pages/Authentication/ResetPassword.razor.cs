using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
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
using Notes2022RCL.Comp;
using W8lessLabs.Blazor.LocalFiles;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.LinearGauge;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.SplitButtons;
using Syncfusion.Blazor.Calendars;
using System.ComponentModel.DataAnnotations;

namespace Notes2022RCL.Pages.Authentication
{
    public partial class ResetPassword
    {

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password2 { get; set; }
        }

        [Parameter] public string payload { get; set; }

        protected InputModel Input = new()
        { Password = string.Empty, Password2 = string.Empty };

        ConfirmEmailRequest mess { get; set; }

        public string Message { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            mess = (ConfirmEmailRequest)System.Text.Json.JsonSerializer.Deserialize(Globals.Base64Decode(payload), typeof(ConfirmEmailRequest));

        }

        public async Task GotoChange()
        {
            if (Input.Password != Input.Password2)
            {
                Message = "Passwords do not match!";
                return;
            }

            ResetPasswordRequest req = new() {UserId = mess.UserId, Code = mess.Code, NewPassword = Input.Password};

            AuthReply reply = await AuthClient.ResetPassword2Async(req);

            Message = reply.Message;
            StateHasChanged();
        }

    }
}