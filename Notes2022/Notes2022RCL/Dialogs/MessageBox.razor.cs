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
using System.Timers;

namespace Notes2022RCL.Dialogs
{
    public partial class MessageBox
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the message input.
        /// </summary>
        /// <value>The message input.</value>
        [Parameter]
        public string MessageInput { get; set; }

        [Parameter]
        public double TimeToClose { get; set; } = 0;

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        System.Timers.Timer ticker;
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender && TimeToClose > 0)
            {
                ticker = new System.Timers.Timer(TimeToClose);
                ticker.Elapsed += AutoClose;
                ticker.Enabled = true;
                ticker.Start();
            }
            base.OnAfterRender(firstRender);
        }

        protected void AutoClose(Object source, ElapsedEventArgs e)
        {
            Cancel();
        }
    }
}
