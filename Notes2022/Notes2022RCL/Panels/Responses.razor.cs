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
using Notes2022.Proto;

namespace Notes2022RCL.Panels
{
    public partial class Responses
    {
        /// <summary>
        /// List of response headers
        /// </summary>
        /// <value>The headers.</value>
        [Parameter]
        public List<NoteHeader> Headers { get; set; }

        /// <summary>
        /// Show content for responses
        /// </summary>
        /// <value><c>true</c> if [show content r]; otherwise, <c>false</c>.</value>
        [Parameter]
        public bool ShowContentR { get; set; }

        /// <summary>
        /// Expand all rows
        /// </summary>
        /// <value><c>true</c> if [expand all r]; otherwise, <c>false</c>.</value>
        [Parameter]
        public bool ExpandAllR { get; set; }

        //[Parameter] public NoteIndex Parent {get; set;}
        //public bool ShowContent { get; set; }
        //public bool ExpandAll { get; set; }
        /// <summary>
        /// Gets or sets the sf grid2.
        /// </summary>
        /// <value>The sf grid2.</value>
        protected SfGrid<NoteHeader> sfGrid2 { get; set; }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject]
        NavigationManager Navigation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref = "Responses"/> class.
        /// </summary>
        public Responses()
        {
        }

        /// <summary>
        /// Copy parameter to local copy
        /// </summary>
         //protected override async Task OnParametersSetAsync()
        //{
        //}
        public void DataBoundHandler()
        {
            // Expand if appropriate
            if (ExpandAllR)
            {
                sfGrid2.ExpandAllDetailRowAsync().GetAwaiter();
            }
        }

        //System.Timers.Timer timer2;
        //protected void ActionCompleteHandler(ActionEventArgs<NoteHeader> action)
        //{
        //    if (action.RequestType == Syncfusion.Blazor.Grids.Action.Filtering)
        //    {
        //        timer2 = new System.Timers.Timer(1000);
        //        timer2.Elapsed += TimerTick2;
        //        timer2.Enabled = true;
        //    }
        //}
        //protected void TimerTick2(Object source, ElapsedEventArgs e)
        //{
        //    timer2.Elapsed -= TimerTick2;
        //    timer2.Stop();
        //    timer2.Enabled = false;
        //    this.StateHasChanged();
        //}
        /// <summary>
        /// GO show the note
        /// </summary>
        /// <param name = "args">The arguments.</param>
        protected void DisplayIt(RowSelectEventArgs<NoteHeader> args)
        {
            Navigation.NavigateTo("notedisplay/" + args.Data.Id);
        }
    }
}