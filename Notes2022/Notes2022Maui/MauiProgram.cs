using Blazored.Modal;
using Blazored.SessionStorage;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Configuration;
using Notes2022.Proto;
using Notes2022RCL.Comp;
using Syncfusion.Blazor;

namespace Notes2022Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            //ConfigurationManager configuration = builder.Configuration;

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            Notes2022RCL.Globals.IsMaui = true;

            //builder.Services.AddSingleton<WeatherForecastService>();

            builder.Services.AddBlazoredModal();
            builder.Services.AddBlazoredSessionStorage();
            builder.Services.AddSingleton<CookieStateAgent>();       // for login state mgt = "myState" injection in _imports.razor

            builder.Services.AddSyncfusionBlazor();     // options => { options.IgnoreScriptIsolation = true; });

            string baseUri = "https://localhost:7133/"; // "https://www.drsinder.com:448"; "https://localhost:7133/"; 
            Notes2022RCL.Globals.ServerAddress = baseUri;

            // Add my gRPC service so it can be injected.
            builder.Services.AddSingleton(services =>
            {
                HttpClient? httpClient = new(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 50 * 1024 * 1024 });
                return new Notes2022Server.Notes2022ServerClient(channel);
            });


            return builder.Build();
        }
    }
}