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
using Notes2022.Shared;

namespace Notes2022RCL.Dialogs
{
    public partial class AccessCheckBox
    {
        /// <summary>
        /// The item and its full toekn
        /// </summary>
        /// <value>The model.</value>
        [Parameter]
        public AccessItem Model { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref = "AccessCheckBox"/> class.
        /// </summary>
        public AccessCheckBox()
        {
        }

        /// <summary>
        /// Invert checked state and update
        /// </summary>
        protected async Task OnClick()
        {
            Model.isChecked = !Model.isChecked;
            switch (Model.which)
            {
                case AccessX.ReadAccess:
                {
                    Model.Item.ReadAccess = Model.isChecked;
                    break;
                }

                case AccessX.Respond:
                {
                    Model.Item.Respond = Model.isChecked;
                    break;
                }

                case AccessX.Write:
                {
                    Model.Item.Write = Model.isChecked;
                    break;
                }

                case AccessX.DeleteEdit:
                {
                    Model.Item.DeleteEdit = Model.isChecked;
                    break;
                }

                case AccessX.SetTag:
                {
                    Model.Item.SetTag = Model.isChecked;
                    break;
                }

                case AccessX.ViewAccess:
                {
                    Model.Item.ViewAccess = Model.isChecked;
                    break;
                }

                case AccessX.EditAccess:
                {
                    Model.Item.EditAccess = Model.isChecked;
                    break;
                }

                default:
                    break;
            }

            _ = await Client.UpdateAccessItemAsync(Model.Item, myState.AuthHeader);
        }
    }
}