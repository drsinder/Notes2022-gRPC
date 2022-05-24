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
using W8lessLabs.Blazor.LocalFiles;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.LinearGauge;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.SplitButtons;
using Syncfusion.Blazor.Calendars;
using System.Web;
using System.Text.Json;

namespace Notes2022RCL.Comp
{
    public partial class CookieStateAgent
    {
        private LoginReply? savedLoginValue;
        protected override async Task OnParametersSetAsync()
        {
            if (myState.IsAuthenticated) // nothing to do here!
                return;
            savedLoginValue = myState.LoginReply;
            try
            {
                //if (savedLogin == null && JS != null)
                {
                    await LoginReplyAsync(); // try to get a cookie to auth
                }
            }
            catch (Exception)
            {
            }
        }

        protected async Task LoginReplyAsync()
        {
            //if (savedLogin == null && JS != null)
            {
                try
                {
                    // JS injected in .razor file
                    IJSObjectReference module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");
                    string cookie = await ReadCookies(module);
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        // found a cookie
                        string json = HttpUtility.HtmlDecode(Globals.Base64Decode(cookie));
                        savedLoginValue = JsonSerializer.Deserialize<LoginReply>(json);
                        myState.LoginReply = savedLoginValue;
                    }

                    await module.DisposeAsync();
                }
                catch (Exception ex)
                {
                    string x = ex.Message;
                }
            }
        }

        private async Task<string> ReadCookies(IJSObjectReference module)
        {
            try
            {
                string cookie = await module.InvokeAsync<string>("ReadCookie", Globals.Cookie);
                return cookie;
            }
            catch (Exception)
            {
            }

            return String.Empty;
        }
    }
}