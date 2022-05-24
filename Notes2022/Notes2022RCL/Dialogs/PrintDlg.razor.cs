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
using System.Timers;
using Syncfusion.Blazor.RichTextEditor;

namespace Notes2022RCL.Dialogs
{
    public partial class PrintDlg
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the print stuff.
        /// </summary>
        /// <value>The print stuff.</value>
        [Parameter]
        public string PrintStuff { get; set; }

#pragma warning disable IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets a value indicating whether [readonly print].
        /// </summary>
        /// <value><c>true</c> if [readonly print]; otherwise, <c>false</c>.</value>
        private bool readonlyPrint { get; set; }

        /// <summary>
        /// The rte object
        /// </summary>
        SfRichTextEditor RteObj;
        /// <summary>
        /// Gets or sets the timer2.
        /// </summary>
        /// <value>The timer2.</value>
        private System.Timers.Timer timer2 { get; set; }

#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles

        /// <summary>
        /// Ons the print.
        /// </summary>
        private void onPrint()
#pragma warning restore IDE1006 // Naming Styles

        {
            RteObj.Print();
        }

        /// <summary>
        /// Method invoked after each time the component has been rendered.
        /// </summary>
        /// <param name = "firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)"/> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name = "firstRender"/> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                timer2 = new System.Timers.Timer(500);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

                timer2.Elapsed += TimerTick2;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

                timer2.Enabled = true;
            }
        }

        /// <summary>
        /// Timers the tick2.
        /// </summary>
        /// <param name = "source">The source.</param>
        /// <param name = "e">The <see cref = "ElapsedEventArgs"/> instance containing the event data.</param>
        protected void TimerTick2(Object source, ElapsedEventArgs e)
        {
            timer2.Enabled = false;
            timer2.Stop();
            readonlyPrint = false;
            this.RteObj.ExecuteCommand(CommandName.InsertHTML, PrintStuff);
            readonlyPrint = true;
            StateHasChanged();
        }

        /// <summary>
        /// Closes the print.
        /// </summary>
        private void ClosePrint()
        {
            ModalInstance.CancelAsync();
        }
    }
}