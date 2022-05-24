using System.Linq;
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notes2022.Shared;
using Syncfusion.Blazor.DropDowns;
using System.IO;
using System.Text;

namespace Notes2022RCL.Dialogs
{
    public partial class Copy
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        [Parameter]
        public NoteHeader Note { get; set; }

        //[Parameter] public UserData UserData { get; set; }
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        private NoteFileList Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [whole string].
        /// </summary>
        /// <value><c>true</c> if [whole string]; otherwise, <c>false</c>.</value>
        private bool WholeString { get; set; }

        /// <summary>
        /// Gets or sets the selected identifier.
        /// </summary>
        /// <value>The selected identifier.</value>
        private int SelectedId { get; set; } = 0;
        /// <summary>
        /// On initialized as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected async override Task OnInitializedAsync()
        {
            Files = await Client.GetNoteFilesOrderedByNameAsync(new NoRequest(), myState.AuthHeader);
            Files.List.Insert(0, new NoteFile{Id = 0, NoteFileName = "Select a file"});
        }

        /// <summary>
        /// Called when [submit].
        /// </summary>
        protected async Task OnSubmit()
        {
            if (SelectedId == 0)
                return;
            CopyModel cm = new CopyModel();
            cm.FileId = SelectedId;
            cm.Note = Note;
            cm.WholeString = WholeString;
            //cm.UserData = UserData;
            await Client.CopyNoteAsync(cm, myState.AuthHeader);
            await ModalInstance.CloseAsync();
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }
    }
}