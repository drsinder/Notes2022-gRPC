using Microsoft.AspNetCore.Components;
using Blazored.Modal;
using Microsoft.AspNetCore.SignalR.Client;

namespace Notes2022RCL.Dialogs
{
    public partial class TalkDialog
    {
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        [Parameter]
        public HubConnection Hub { get; set; }

        [Parameter]
        public string ToclientId { get; set; }

        [Parameter]
        public string FromclientId { get; set; }

        [Parameter]
        public string userName { get; set; }

        [Parameter]
        public string toName { get; set; }

        protected string messageInput { get; set; }

        protected List<string> messages { get; set; } = new();

        private DateTime lastUpdate { get; set; }

        TimeSpan minUpdate = TimeSpan.FromMilliseconds(400);

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// Setup Hub handlers. 
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        protected override Task OnParametersSetAsync()
        {
            lastUpdate = DateTime.Now;

            Hub.On<string, string>("PrivateMessage", (userName, message) =>
            {
                if (DateTime.Now - lastUpdate < minUpdate)
                    return;

                lastUpdate = DateTime.Now;
                messages.Add($"{userName}: {message}");
                if (messages.Count > 15) 
                    messages.RemoveAt(0);
                StateHasChanged();
            });

            Hub.On("EndTalk", () =>
            {
                ModalInstance.CancelAsync();

                myState.ShowMessage("Talk ended!");
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Send end talk message to server.
        /// </summary>
        private async Task Cancel()
        {
            await Hub.SendAsync("EndTalk", ToclientId, FromclientId);
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        private async Task Send()
        {
            await Hub.SendAsync("PrivateMessage", ToclientId, FromclientId, myState.UserInfo.Displayname, messageInput);
            messageInput = String.Empty;
        }
    }
}