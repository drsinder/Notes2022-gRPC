using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    public partial class Chat
    {
        private HubConnection? hubConnection;
        private List<string> messages = new List<string>();
        private string? messageInput;
        protected override async Task OnInitializedAsync()
        {
            AString vdir = await Client.GetTextFileAsync(new AString() { Val = "AppVirtDir" });
            string pURI = vdir.Val + "/chathub";

            hubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri(pURI)).Build();
            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                if (messages.Count > 20)
                    messages.RemoveAt(0);
                string? encodedMsg = $"{user}: {message}";
                messages.Add(encodedMsg);
                StateHasChanged();
            });
            await hubConnection.StartAsync();
        }

        private async Task Send()
        {
            if (hubConnection is not null)
            {
                await hubConnection.SendAsync("SendMessage", myState.UserInfo.Displayname, myState.UserInfo.Subject, messageInput);
            }
        }

        public bool IsConnected => hubConnection?.State == HubConnectionState.Connected;
        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}