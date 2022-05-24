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
using Notes2022.Shared;

namespace Notes2022RCL.Dialogs
{
    public partial class AccessList
    {
        /// <summary>
        /// Gets or sets the modal.
        /// </summary>
        /// <value>The modal.</value>
        [CascadingParameter]
        public IModalService Modal { get; set; }

        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// File Id we are working on
        /// </summary>
        /// <value>The file identifier.</value>
        
#pragma warning disable IDE1006 // Naming Styles

        [Parameter]
        public int fileId { get; set; }

        /// <summary>
        /// Grid of tokens
        /// </summary>
        private SfGrid<NoteAccess> MyGrid;
        /// <summary>
        /// List of tokens
        /// </summary>
        /// <value>My list.</value>
        private List<NoteAccess> myList { get; set; }

        /// <summary>
        /// Temp list of tokens
        /// </summary>
        /// <value>The user list.</value>
        private List<GAppUser> userList { get; set; }

        /// <summary>
        /// My access
        /// </summary>
        /// <value>My access.</value>
        private NoteAccess myAccess { get; set; }

        /// <summary>
        /// Gets or sets the arc identifier.
        /// </summary>
        /// <value>The arc identifier.</value>
        private int arcId { get; set; }

        /// <summary>
        /// message to display
        /// </summary>
        /// <value>The message.</value>
        private string message { get; set; }

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

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Initializes a new instance of the <see cref = "AccessList"/> class.
        /// </summary>
        public AccessList()
        {
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected async override Task OnParametersSetAsync()
        {
            arcId = await sessionStorage.GetItemAsync<int>("ArcId");
            AccessAndUserList myLists = await Client.GetAccessAndUserListAsync(new AccessAndUserListRequest()
            {FileId = fileId, ArcId = arcId, UserId = myState.UserInfo?.Subject}, myState.AuthHeader);
            myList = myLists.AccessList.ToList();
            userList = myLists.AppUsers.List.ToList();
            myAccess = myLists.UserAccess;
        }

        /// <summary>
        /// We are done
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Add a new token for another user
        /// </summary>
        protected async void CreateNew()
        {
            var parameters = new ModalParameters();
            parameters.Add("userList", userList);
            parameters.Add("NoteFileId", fileId);
            var xx = Modal.Show<AddAccessDlg>("", parameters);
            await xx.Result;
            try
            {
                await this.InvokeAsync(() =>  this.StateHasChanged());
            }
            catch (Exception) { }
            await MyGrid.Refresh();
        }

        /// <summary>
        /// Item deleted - refresh list
        /// </summary>
        /// <param name = "newMessage">The new message.</param>
        protected async Task ClickHandler(string newMessage)
        {
            arcId = await sessionStorage.GetItemAsync<int>("ArcId");
            NoteAccessList myLists = await Client.GetAccessListAsync(new AccessAndUserListRequest()
            {FileId = fileId, ArcId = arcId, UserId = myState.UserInfo?.Subject}, myState.AuthHeader);
            myList = myLists.List.ToList();
            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
            await MyGrid.Refresh();
        }
    }
}