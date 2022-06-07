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

        /// <summary>
        /// On after render as an asynchronous operation.
        /// First time setup 5 second update of page.
        /// </summary>
        /// <param name="firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                Reloader = new Timer(5000);
                Reloader.Elapsed += Replot;
                Reloader.Enabled = true;
                Reloader.Start();
            }
        }

        /// <summary>
        /// Reloads the page.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        protected void Replot(Object source, ElapsedEventArgs e)
        {
            StateHasChanged();
        }

        /// <summary>
        /// Request to talk to someone.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected async Task Talk(RowSelectEventArgs<ActiveUsers> args)
        {
            await myState.InitTalk(args.Data.ClientId, args.Data.DisplayName);
        }
    }
}