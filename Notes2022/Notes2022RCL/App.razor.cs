//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Web;
//using System.Net.Http;
//using System.Net.Http.Json;
//using Microsoft.AspNetCore.Components.Forms;
//using Microsoft.AspNetCore.Components.Web.Virtualization;
//using Microsoft.AspNetCore.Components.WebAssembly.Http;
//using Microsoft.JSInterop;
//using Notes2022RCL;
//using Notes2022.Proto;
//using Blazored;
//using Blazored.Modal;
//using Blazored.Modal.Services;
//using Notes2022RCL.Comp;
//using W8lessLabs.Blazor.LocalFiles;
//using Syncfusion.Blazor;
//using Syncfusion.Blazor.Navigations;
//using Syncfusion.Blazor.Buttons;
//using Syncfusion.Blazor.Grids;
//using Syncfusion.Blazor.LinearGauge;
//using Syncfusion.Blazor.Inputs;
//using Syncfusion.Blazor.SplitButtons;
//using Syncfusion.Blazor.Calendars;
//using Microsoft.AspNetCore.Components.Routing;
//using Notes2022RCL.Shared;
//using Syncfusion.Licensing;

//namespace Notes2022RCL
//{
//    public partial class App
//    {
//        protected override async Task OnParametersSetAsync()
//        {
//            AString key = await Client.GetTextFileAsync(new AString()
//            { Val = "syncfusionkey.rsghjjsrsrj43632353" });

//            SyncfusionLicenseProvider.RegisterLicense(key.Val);
//        }

//        }
//    }

