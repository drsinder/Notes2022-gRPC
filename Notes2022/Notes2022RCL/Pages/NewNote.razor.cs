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
    public partial class NewNote
    {
        /// <summary>
        /// Gets or sets the notesfile identifier.
        /// </summary>
        /// <value>The notesfile identifier.</value>
        [Parameter]
        public int NotesfileId { get; set; }

        /// <summary>
        /// Gets or sets the base note header identifier.
        /// </summary>
        /// <value>The base note header identifier.</value>
        [Parameter]
        public long BaseNoteHeaderId { get; set; } //  base note we are responding to

        /// <summary>
        /// Gets or sets the reference identifier.
        /// </summary>
        /// <value>The reference identifier.</value>
        [Parameter]
        public long RefId { get; set; } //  what we are responding to

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        protected TextViewModel Model { get; set; } = new TextViewModel();
        /// <summary>
        /// Gets or sets the HTTP.
        /// </summary>
        /// <value>The HTTP.</value>
        [Inject]
        HttpClient Http { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Initializes a new instance of the <see cref = "NewNote"/> class.
        /// </summary>
        public NewNote()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        {
        }

        /// <summary>
        /// Just fill in a few fields and we are ready...
        /// </summary>
        protected override void OnParametersSet()
        {
            Model.NoteFileID = NotesfileId; // which file?
            Model.NoteID = 0; // 0 for new note
            Model.BaseNoteHeaderID = BaseNoteHeaderId; // base note we are responding to
            Model.RefId = RefId; // note we are responding to
            Model.MyNote = "";
            Model.MySubject = "";
            Model.TagLine = "";
            Model.DirectorMessage = "";
        }
    }
}