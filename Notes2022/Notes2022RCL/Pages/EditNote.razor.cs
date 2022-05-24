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
using Notes2022RCL.Panels;

namespace Notes2022RCL.Pages
{
    public partial class EditNote
    {
        /// <summary>
        /// Gets or sets the note identifier.
        /// </summary>
        /// <value>The note identifier.</value>
        [Parameter]
        public long NoteId { get; set; } //  what we are editing

        /// <summary>
        /// our data for the note in edit model
        /// </summary>
        /// <value>The model.</value>
        protected TextViewModel Model { get; set; } = new TextViewModel();
        /// <summary>
        /// A note display model
        /// </summary>
        /// <value>The stuff.</value>
        protected DisplayModel stuff { get; set; }

        /// <summary>
        /// The go
        /// </summary>
        protected bool go = false;
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref = "EditNote"/> class.
        /// </summary>
        public EditNote()
        {
        }

        // get all the data
        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            stuff = await Client.GetNoteContentAsync(new DisplayModelRequest()
            {NoteId = NoteId, Vers = 0}, myState.AuthHeader);
            Model.NoteFileID = stuff.NoteFile.Id;
            Model.NoteID = NoteId;
            Model.BaseNoteHeaderID = stuff.Header.BaseNoteId;
            Model.RefId = stuff.Header.RefId;
            Model.MyNote = stuff.Content.NoteBody;
            Model.MySubject = stuff.Header.NoteSubject;
            Model.DirectorMessage = stuff.Header.DirectorMessage;
            string tags = "";
            foreach (var tag in stuff.Tags)
            {
                tags += tag + " ";
            }

            Model.TagLine = tags;
            go = true;
        }
    }
}