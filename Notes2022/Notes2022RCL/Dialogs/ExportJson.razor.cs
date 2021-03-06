// ***********************************************************************
// Assembly         : Notes2022RCL
// Author           : Dale Sinder
// Created          : 05-24-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-25-2022
//
// Copyright ? 2022, Dale Sinder
//
// Name: ExportJson.razor.cs
//
// Description:
//      TODO
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 3 as
// published by the Free Software Foundation.   
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License version 3 for more details.
//
//  You should have received a copy of the GNU General Public License
//  version 3 along with this program in file "license-gpl-3.0.txt".
//  If not, see<http://www.gnu.org/licenses/gpl-3.0.txt>.
// ***********************************************************************
// <copyright file="ExportJson.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Notes2022.Proto;
using Notes2022RCL.Comp;
using System.Text;

namespace Notes2022RCL.Dialogs
{
    /// <summary>
    /// Class ExportJson.
    /// Implements the <see cref="ComponentBase" />
    /// Implements the <see cref="System.IAsyncDisposable" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    /// <seealso cref="System.IAsyncDisposable" />
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
        /// Gets or sets a value indicating whether this <see cref="ExportJson" /> is marked.
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
            { FileId = nfid, ArcId = 0, NestResponses = true }, myState.AuthHeader);
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
        /// <param name="firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
#if (!MAUI)
            if (firstRender && !Globals.IsMaui)
            {
                module = await JS.InvokeAsync<IJSObjectReference>("import", "./scripts.js");
            }
#endif
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
        /// <param name="filename">The filename.</param>
        /// <param name="data">The data.</param>
        public async Task SaveAs(string filename, byte[] data)
        {
#if MAUI
            if (Globals.IsMaui)
            {
                GAppUser ui = await Client.GetUserDataAsync(new(), myState.AuthHeader);
                Notes2022MauiLib.MauiFileActions mauiFileActions = new Notes2022MauiLib.MauiFileActions();
                string fn = await mauiFileActions.SaveToFileAndClipBoard(filename, data, ui.Pref1);
                model.myMenu.ExportDone(fn);
            }
            else
#endif
            {
#pragma warning disable CS8604 // Possible null reference argument.

                await module.InvokeVoidAsync("saveAsFile", filename, Convert.ToBase64String(data));
#pragma warning restore CS8604 // Possible null reference argument.
            }

        }
    }
}