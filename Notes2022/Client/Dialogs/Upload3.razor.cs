using Notes2022.Client;
using Notes2022.Client.Shared;
using Notes2022.Proto;
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
using Microsoft.AspNetCore.Components;
using System.Text;
using W8lessLabs.Blazor.LocalFiles;

namespace Notes2022.Client.Dialogs
{
    /// <summary>
    /// Class Upload1.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class Upload3
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>The filename.</value>
        public byte[] contents { get; set; }

        public string filename { get; set; }


        FileSelect fileSelect;

        // Handle the file selection event
        async Task FilesSelectedHandler(SelectedFile[] selectedFiles)
        {
            // example of opening a selected file...
            var selectedFile = selectedFiles[0];
            // alternatively, load all the bytes at once
            byte[] fileBytes = await fileSelect.GetFileBytesAsync(selectedFile.Name);
            await ModalInstance.CloseAsync(ModalResult.Ok(fileBytes));
        }

        /// <summary>
        /// Oks this instance.
        /// </summary>
        private void Ok()
        {
            //contents = Encoding.ASCII.GetBytes(filename);

            //ModalInstance.CloseAsync(ModalResult.Ok(contents));


            fileSelect.SelectFiles();
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