using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Notes2022RCL.Pages
{
    public partial class Chat
    {
        private HubConnection? hubConnection;
        private List<string> messages = new List<string>();
        private string? messageInput;
        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri("/chathub")).Build();
            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var encodedMsg = $"{user}: {message}";
                messages.Add(encodedMsg);
                StateHasChanged();
            });
            await hubConnection.StartAsync();
        }

        private async Task Send()
        {
            if (hubConnection is not null)
            {
                await hubConnection.SendAsync("SendMessage", myState.UserInfo.Displayname, messageInput);
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