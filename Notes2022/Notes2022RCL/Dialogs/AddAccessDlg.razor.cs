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
using Notes2022.Proto;
using System.Timers;

namespace Notes2022RCL.Dialogs
{
    public partial class AddAccessDlg
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the user list.
        /// </summary>
        /// <value>The user list.</value>
        [Parameter]
        public List<GAppUser> userList { get; set; }

        /// <summary>
        /// Gets or sets the note file identifier.
        /// </summary>
        /// <value>The note file identifier.</value>
        [Parameter]
        public int NoteFileId { get; set; }

        /// <summary>
        /// Gets or sets the selected user identifier.
        /// </summary>
        /// <value>The selected user identifier.</value>
        protected string selectedUserId { get; set; }

        /// <summary>
        /// Gets or sets the delay.
        /// </summary>
        /// <value>The delay.</value>
        protected System.Timers.Timer delay { get; set; }

        /// <summary>
        /// Gets or sets the arc string.
        /// </summary>
        /// <value>The arc string.</value>
        protected string ArcString { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Gets or sets the session storage.
        /// </summary>
        /// <value>The session storage.</value>
        [Inject]
        Blazored.SessionStorage.ISessionStorageService sessionStorage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref = "AddAccessDlg"/> class.
        /// </summary>
        public AddAccessDlg()
        {
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            selectedUserId = "none";
        //if (NoteFile is null)
        //    Cancel();
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        private async Task Create()
        {
            if (selectedUserId != "none")
            {
                int aId = await sessionStorage.GetItemAsync<int>("ArcId");
                NoteAccess item = new();
                item.UserID = selectedUserId;
                item.NoteFileId = NoteFileId;
                item.ArchiveId = aId;
                // all access options left false
                _ = await Client.AddAccessItemAsync(item, myState.AuthHeader);
                delay = new(250); // allow time for server to respond
                delay.Enabled = true;
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

                delay.Elapsed += Done; // then close the dialog automatically
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

                return;
            }

            await ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Dones the specified source.
        /// </summary>
        /// <param name = "source">The source.</param>
        /// <param name = "e">The <see cref = "ElapsedEventArgs"/> instance containing the event data.</param>
        public void Done(Object source, ElapsedEventArgs e)
        {
            delay.Enabled = false;
            delay.Stop();
            ModalInstance.CancelAsync();
        }
    }
}