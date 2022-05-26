using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Dialogs
{
    public partial class Upload
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        [Parameter]
        public string NoteFile { get; set; }

        /// <summary>
        /// Gets or sets the upload data.
        /// </summary>
        /// <value>The upload file.</value>
        [Parameter]
        public string UploadText { get; set; }

        /// <summary>
        /// On after render as an asynchronous operation.
        /// </summary>
        /// <param name = "firstRender">if set to <c>true</c> [first render].</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var request = new ImportRequest()
                { NoteFile = NoteFile, Payload = UploadText };
                _ = await Client.ImportJsonAsync(request, myState.AuthHeader, deadline: DateTime.UtcNow.AddMinutes(10));
                await ModalInstance.CancelAsync();
            }
        }
    }
}