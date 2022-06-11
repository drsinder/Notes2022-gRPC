using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Notes2022RCL.Dialogs;

namespace Notes2022RCL.Comp
{
    public partial class MasterHubClient
    {
        [CascadingParameter]
        public IModalService Modal { get; set; }

        [Parameter]
        public CookieStateAgent Host { get; set; }

        protected override async Task OnParametersSetAsync()
        {   // connect to hub
            Host.MasterHubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri("/masterhub")).Build();

            // handle talk requests
            Host.MasterHubConnection.On<string, string, string, string>("TalkRequest", async (ToclientId, FromclientId, userName, toName) =>
            {
                // informs user of talk request and asks if they want to accept
                IModalReference? ret = ShowYesNo($"{userName} wants to talk.");
                ModalResult x = await ret.Result;
                if (x.Cancelled)
                {
                    await myState.MasterHubConnection?.SendAsync("TalkRejected", ToclientId, FromclientId, toName);
                    return;
                }
                // tell hub talk was accepted.  it will create a group and initiate talk dialogs.
                await myState.MasterHubConnection?.SendAsync("TalkAccepted", ToclientId, FromclientId, userName, toName);
            });

            // Show the talk dailog
            Host.MasterHubConnection.On<string, string, string, string>("TalkAccepted", (ToclientId, FromclientId, userName, toName) =>
            {
                ModalParameters? parameters = new();
                parameters.Add("ToclientId", ToclientId);
                parameters.Add("FromclientId", FromclientId);
                parameters.Add("userName", userName);
                parameters.Add("toName", toName);
                parameters.Add("Hub", Host.MasterHubConnection);

                Modal.Show<TalkDialog>("Talk", parameters);
            });

            // tell caller the call was rejected
            Host.MasterHubConnection.On<string>("TalkRejected", (message) =>
            {
                ShowMessage(message);
            });

            // receive active user list and count periodically
            Host.MasterHubConnection.On("ReceiveActiveUsers", (Action<int, List<ActiveUsers>>)((count, users) =>
            {
                Host.ActiveUsers = users;
                Host.UserCount = count;
            }));

            // starts the hub connection
            await Host.MasterHubConnection.StartAsync();

        }

        private DateTime LastUpdate { get; set; }

        private readonly TimeSpan minUpdate = TimeSpan.FromMilliseconds(400);

        /// <summary>
        /// Handle state change.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ShowMessage(string message)
        {
            if (DateTime.Now - LastUpdate < minUpdate)
                return;

            LastUpdate = DateTime.Now;

            ModalParameters? parameters = new();
            parameters.Add("MessageInput", message);
            Modal.Show<MessageBox>("", parameters);
        }

        private IModalReference? ShowYesNo(string message)
        {
            ModalParameters? parameters = new();
            parameters.Add("MessageInput", message);
            IModalReference? retval = Modal.Show<YesNo>("", parameters);
            return retval;
        }

        /// <summary>
        /// Initializes the talk.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="userName">Name of the user.</param>
        public async Task InitTalk(string clientId, string userName)
        {
            string? FromId = Host.MasterHubConnection?.ConnectionId;

            await myState.MasterHubConnection?.SendAsync("TalkRequest", clientId, FromId, myState.UserInfo.Displayname, userName);

            ModalParameters? parameters = new();
            parameters.Add("MessageInput", "Requesting talk...");
            parameters.Add("TimeToClose", 1500D);
            Modal.Show<MessageBox>("talk", parameters);
        }

    }
}