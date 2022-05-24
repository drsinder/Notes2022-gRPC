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
using Notes2022.Shared;
using System.Text;
using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    public partial class Preferences
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the user data.
        /// </summary>
        /// <value>The user data.</value>
        private GAppUser UserData { get; set; }

        /// <summary>
        /// Gets or sets the current text.
        /// </summary>
        /// <value>The current text.</value>
        private string currentText { get; set; }

        /// <summary>
        /// Gets or sets my sizes.
        /// </summary>
        /// <value>My sizes.</value>
        private List<LocalModel2> MySizes { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        private string pageSize { get; set; }

        private string NewCheck { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// On initialized as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            UserData = await Client.GetUserDataAsync(new NoRequest(), myState.AuthHeader);
            pageSize = UserData.Ipref2.ToString();
            NewCheck = UserData.Ipref0.ToString();
            MySizes = new List<LocalModel2>{new LocalModel2("0", "All"), new LocalModel2("5"), new LocalModel2("10"), new LocalModel2("12"), new LocalModel2("20")};
            currentText = " ";
        }

        /// <summary>
        /// Called when [submit].
        /// </summary>
        private async Task OnSubmit()
        {
            UserData.Ipref2 = int.Parse(pageSize);
            Globals.Interval = UserData.Ipref0 = int.Parse(NewCheck);
            await Client.UpdateUserDataAsync(UserData, myState.AuthHeader);
            Navigation.NavigateTo("");
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            Navigation.NavigateTo("");
        }

        /// <summary>
        /// Class LocalModel2.
        /// </summary>
        public class LocalModel2
        {
            /// <summary>
            /// Initializes a new instance of the <see cref = "LocalModel2"/> class.
            /// </summary>
            /// <param name = "psize">The psize.</param>
            public LocalModel2(string psize)
            {
                Psize = psize;
                Name = psize;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref = "LocalModel2"/> class.
            /// </summary>
            /// <param name = "psize">The psize.</param>
            /// <param name = "name">The name.</param>
            public LocalModel2(string psize, string name)
            {
                Psize = psize;
                Name = name;
            }

            /// <summary>
            /// Gets or sets the psize.
            /// </summary>
            /// <value>The psize.</value>
            public string Psize { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }
        }
    }
}