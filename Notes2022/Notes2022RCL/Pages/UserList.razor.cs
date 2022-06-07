using Microsoft.AspNetCore.SignalR.Client;
using Notes2022.Proto;
using Syncfusion.Blazor.Grids;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Notes2022RCL.Pages
{
    public partial class UserList
    {
        private Timer Reloader;
        public SfGrid<ActiveUsers> sfGrid1 { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Reloader = new Timer(5000);
                Reloader.Elapsed += Reload;
                Reloader.Enabled = true;
                Reloader.Start();
            }
        }

        protected async void Reload(Object source, ElapsedEventArgs e)
        {
            StateHasChanged();
        }

        protected async Task Talk(RowSelectEventArgs<ActiveUsers> args)
        {
            await myState.InitTalk(args.Data.ClientId, args.Data.DisplayName);
        }
    }
}