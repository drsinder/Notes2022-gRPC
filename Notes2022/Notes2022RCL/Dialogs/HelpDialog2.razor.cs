using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Dialogs
{
    public partial class HelpDialog2
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// The text
        /// </summary>
        private string text = string.Empty;
        /// <summary>
        /// Get some simple stuff from server
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            text = (await Client.GetTextFileAsync(new AString()
            { Val = "helpdialog2.html" })).Val;
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