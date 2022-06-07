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
using Microsoft.AspNetCore.SignalR.Client;

namespace Notes2022RCL.Dialogs
{
    public partial class TalkDialog
    {
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        [Parameter]
        public HubConnection Hub { get; set; }

        [Parameter]
        public string ToclientId { get; set; }

        [Parameter]
        public string FromclientId { get; set; }

        [Parameter]
        public string userName { get; set; }

        [Parameter]
        public string toName { get; set; }

        protected string messageInput { get; set; }

        protected List<string> messages { get; set; } = new();

        private DateTime lastUpdate { get; set; }

        TimeSpan minUpdate = TimeSpan.FromMilliseconds(400);

        protected override Task OnParametersSetAsync()
        {
            lastUpdate = DateTime.Now;

            Hub.On<string, string>("PrivateMessage", (userName, message) =>
            {
                if (DateTime.Now - lastUpdate < minUpdate)
                    return;

                lastUpdate = DateTime.Now;
                messages.Add($"{userName}: {message}");
                if (messages.Count > 15) 
                    messages.RemoveAt(0);
                StateHasChanged();
            });

            Hub.On("EndTalk", () =>
            {
                ModalInstance.CancelAsync();
            });

            return Task.CompletedTask;
        }

        //public bool IsConnected => Hub?.State == HubConnectionState.Connected;

        private async Task Cancel()
        {
            await Hub.SendAsync("EndTalk", ToclientId, FromclientId);
        }

        private async Task Send()
        {
            await Hub.SendAsync("PrivateMessage", ToclientId, FromclientId, myState.UserInfo.Displayname, messageInput);
            messageInput = String.Empty;
        }


    }
}