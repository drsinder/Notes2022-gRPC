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
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Notes2022RCL.Dialogs
{
    public partial class ExportJson
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Parameter]
        public ExportViewModel model { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        private string FileName { get; set; }

        /// <summary>
        /// The module
        /// </summary>
        private IJSObjectReference? module;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref = "ExportJson"/> is marked.
        /// </summary>
        /// <value><c>true</c> if marked; otherwise, <c>false</c>.</value>
        private bool marked { get; set; }

        /// <summary>
        /// The message
        /// </summary>
        private string message = "Getting ready...";
        /// <summary>
        /// On initialized as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected async override Task OnInitializedAsync()
        {
            FileName = model.NoteFile.NoteFileName + ".json";
            if (model.Marks is not null && model.Marks.Count > 0)
                marked = true;
            else
                marked = false;
            message = "Exporting " + FileName;
            MemoryStream ms2 = await DoExport();
            await SaveAs(FileName, ms2.GetBuffer());
            ms2.Dispose();
            await ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Does the export.
        /// </summary>
        /// <returns>MemoryStream.</returns>
        private async Task<MemoryStream> DoExport()
        {
            NoteFile nf = model.NoteFile;
            int nfid = nf.Id;
            StringContent stringContent;
            JsonExport stuff;
            stuff = await Client.GetExportJsonAsync(new ExportRequest()
            {FileId = nfid, ArcId = 0, NestResponses = true}, myState.AuthHeader);
            stringContent = new StringContent(JsonConvert.SerializeObject(stuff, Newtonsoft.Json.Formatting.Indented), Encoding.UTF8, "application/json");
            Stream ms0 = await stringContent.ReadAsStreamAsync();
            MemoryStream ms = new MemoryStream();
            await ms0.CopyToAsync(ms);
            ms0.Dispose();
            ms.Close();
            return ms;
        }

        /// <summary>
        /// On after render as an asynchronous operation.
        /// </summary>
        /// <param name = "firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)"/> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name = "firstRender"/> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JS.InvokeAsync<IJSObjectReference>("import", "./scripts.js");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }

        /// <summary>
        /// Saves as.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "data">The data.</param>
        public async Task SaveAs(string filename, byte[] data)
        {
#pragma warning disable CS8604 // Possible null reference argument.

            await module.InvokeVoidAsync("saveAsFile", filename, Convert.ToBase64String(data));
#pragma warning restore CS8604 // Possible null reference argument.

        }
    }
}