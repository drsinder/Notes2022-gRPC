using Microsoft.AspNetCore.Components;
using Blazored.Modal;
using Blazored.Modal.Services;

namespace Notes2022RCL.Dialogs
{
    public partial class YesNo
    {
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the message input.
        /// </summary>
        /// <value>The message input.</value>
        [Parameter]
        public string MessageInput { get; set; }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Oks this instance.
        /// </summary>
        private void Ok()
        {
            ModalInstance.CloseAsync(ModalResult.Ok("Yes"));
        }
    }
}