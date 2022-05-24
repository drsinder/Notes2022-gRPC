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
using Notes2022.Proto;
using Blazored;
using Blazored.Modal;
using Blazored.Modal.Services;
using W8lessLabs.Blazor.LocalFiles;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.LinearGauge;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.SplitButtons;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Buttons;

namespace Notes2022RCL.Shared
{
    public partial class LoginDisplay
    {
        /// <summary>
        /// Begins the sign out.
        /// </summary>
        private void BeginSignOut()
        {
            Navigation.NavigateTo("authentication/logout");
        }

        /// <summary>
        /// Gotoes the profile.
        /// </summary>
        private void GotoProfile()
        {
        //Navigation.NavigateTo("authentication/profile");
        }

        /// <summary>
        /// Gotoes the register.
        /// </summary>
        private void GotoRegister()
        {
            Navigation.NavigateTo("authentication/register");
        }

        /// <summary>
        /// Gotoes the login.
        /// </summary>
        private void GotoLogin()
        {
            Navigation.NavigateTo("authentication/login");
        }

        /// <summary>
        /// Gotoes the home.
        /// </summary>
        private void GotoHome()
        {
            Navigation.NavigateTo("");
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            Globals.LoginDisplay = this;
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public void Reload()
        {
            StateHasChanged();
        }
    }
}