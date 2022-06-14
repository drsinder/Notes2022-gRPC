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

        public HubConnection? MasterHubConnection { get; private set; }

        /// <summary>
        /// Gets the active users.
        /// </summary>
        /// <value>The active users.</value>
        public List<ActiveUsers> ActiveUsers { get; private set; }

        /// <summary>
        /// Gets the user count.
        /// </summary>
        /// <value>The user count.</value>
        public int UserCount { get; private set; }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (MasterHubConnection is not null)
            {
                await MasterHubConnection.DisposeAsync();
                MasterHubConnection = null;
            }
        }


        protected override async Task OnInitializedAsync()
        {   // connect to hub
            string pURI = "/notes2022grpc/masterhub";

#if DEBUG
            pURI = "/masterhub";
#else
            //return;
#endif

            Console.WriteLine("XXXMASTERHUB: " + pURI);
            Console.WriteLine("MASTERHUB: " + NavigationManager.ToAbsoluteUri(pURI));

            try
            {
                MasterHubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri(pURI)).Build();
                // handle talk requests
                MasterHubConnection.On<string, string, string, string>("TalkRequest", async (ToclientId, FromclientId, userName, toName) =>
                {
                    // informs user of talk request and asks if they want to accept
                    IModalReference? ret = ShowYesNo($"{userName} wants to talk.");
                    ModalResult x = await ret.Result;
                    if (x.Cancelled)
                    {
                        await MasterHubConnection?.SendAsync("TalkRejected", ToclientId, FromclientId, toName);
                        return;
                    }
                    // tell hub talk was accepted.  it will create a group and initiate talk dialogs.
                    await MasterHubConnection?.SendAsync("TalkAccepted", ToclientId, FromclientId, userName, toName);
                });

                // Show the talk dailog
                MasterHubConnection.On<string, string, string, string>("TalkAccepted", (ToclientId, FromclientId, userName, toName) =>
                {
                    ModalParameters? parameters = new();
                    parameters.Add("ToclientId", ToclientId);
                    parameters.Add("FromclientId", FromclientId);
                    parameters.Add("userName", userName);
                    parameters.Add("toName", toName);
                    parameters.Add("Hub", MasterHubConnection);

                    Modal.Show<TalkDialog>("Talk", parameters);
                });

                // tell caller the call was rejected
                MasterHubConnection.On<string>("TalkRejected", (message) =>
                {
                    ShowMessage(message);
                });

                // receive active user list and count periodically
                MasterHubConnection.On("ReceiveActiveUsers", (Action<int, List<ActiveUsers>>)((count, users) =>
                {
                    ActiveUsers = users;
                    UserCount = count;
                }));

                // starts the hub connection
                await MasterHubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("MasterHub >>>> " + ex.Message);
            }

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
            string? FromId = MasterHubConnection?.ConnectionId;

            await MasterHubConnection?.SendAsync("TalkRequest", clientId, FromId, myState.UserInfo.Displayname, userName);

            ModalParameters? parameters = new();
            parameters.Add("MessageInput", "Requesting talk...");
            parameters.Add("TimeToClose", 1500D);
            Modal.Show<MessageBox>("talk", parameters);
        }

    }
}