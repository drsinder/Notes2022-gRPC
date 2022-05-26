using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Dialogs
{
    public partial class AccessDeleteButton
    {
        /// <summary>
        /// Gets or sets the note access.
        /// </summary>
        /// <value>The note access.</value>
        [Parameter]
        public NoteAccess noteAccess { get; set; }

        /// <summary>
        /// Gets or sets the on click.
        /// </summary>
        /// <value>The on click.</value>
        [Parameter]
        public EventCallback<string> OnClick { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref = "AccessDeleteButton"/> class.
        /// </summary>
        public AccessDeleteButton()
        {
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        protected async Task Delete()
        {
            await Client.DeleteAccessItemAsync(noteAccess, myState.AuthHeader);
            await OnClick.InvokeAsync("Delete");
        }
    }
}