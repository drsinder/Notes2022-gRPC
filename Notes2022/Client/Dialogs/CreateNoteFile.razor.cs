// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 05-08-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-08-2022
// ***********************************************************************
// <copyright file="CreateNoteFile.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Notes2022.Client;
using Notes2022.Client.Shared;
using Blazored;
using Blazored.Modal;
using Blazored.Modal.Services;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.LinearGauge;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.SplitButtons;
using Syncfusion.Blazor.Calendars;
using System.ComponentModel.DataAnnotations;
using Notes2022.Proto;

namespace Notes2022.Client.Dialogs
{
    /// <summary>
    /// Class CreateNoteFile.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class CreateNoteFile
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>The file identifier.</value>
        [Parameter]
        public int FileId { get; set; }

        /// <summary>
        /// The dummy file
        /// </summary>
        public CreateFileModel dummyFile = new CreateFileModel();
        /// <summary>
        /// Handles the valid submit.
        /// </summary>
        private async Task HandleValidSubmit()
        {
            await Client.CreateNoteFileAsync(new NoteFile()
            {NoteFileName = dummyFile.NoteFileName, NoteFileTitle = dummyFile.NoteFileTitle}, myState.AuthHeader);
            await ModalInstance.CloseAsync();
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Class CreateFileModel.
        /// </summary>
        public class CreateFileModel
        {
            /// <summary>
            /// Gets or sets the name of the note file.
            /// </summary>
            /// <value>The name of the note file.</value>
            [Required]
            public string NoteFileName { get; set; }

            /// <summary>
            /// Gets or sets the note file title.
            /// </summary>
            /// <value>The note file title.</value>
            [Required]
            public string NoteFileTitle { get; set; }
        }
    }
}