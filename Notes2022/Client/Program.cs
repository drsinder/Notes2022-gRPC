// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-19-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-09-2022
// ***********************************************************************
// <copyright file="Program.cs" company="Notes2022.Client">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Blazored.Modal;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Notes2022RCL;
using Notes2022.Proto;
using Syncfusion.Blazor;
using Notes2022RCL.Comp;
using Syncfusion.Licensing;
using Grpc.Core.Interceptors;

WebAssemblyHostBuilder? builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredModal();
builder.Services.AddSingleton<CookieStateAgent>();  // for login state mgt = "myState" injection in _imports.razor

builder.Services.AddSyncfusionBlazor();     // options => { options.IgnoreScriptIsolation = true; });

SyncfusionLicenseProvider.RegisterLicense("NjQ2OTU1QDMyMzAyZTMxMmUzMFBsUmI0QUljc2lmOTlRTHEyVFZMMkZjUmhVU0FkWnBwbWRYRWtkcEQ0ZFU9");

Globals.IsMaui = false;

// Add my gRPC service so it can be injected.
builder.Services.AddSingleton(services =>
{
    Globals.AppVirtDir = ""; // preset for localhost
    string baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
    string[] parts = baseUri.Split('/');
    if (!baseUri.Contains("localhost")) // not localhost - assume it is in a virtual directory ONLY ONE LEVEL DOWN from root of site
    {
        Globals.AppVirtDir = "/" + parts[^2];
    }

    SubdirectoryHandler? handler = new(new HttpClientHandler(), Globals.AppVirtDir);
    GrpcChannel? channel = GrpcChannel.ForAddress(baseUri, 
        new GrpcChannelOptions 
        { HttpHandler = new GrpcWebHandler(handler), MaxReceiveMessageSize = 50 * 1024 * 1024 });   // up to 50MB

    Notes2022Server.Notes2022ServerClient Client = new(channel);
    return Client;
});

await builder.Build().RunAsync();


/// <summary>
/// A delegating handler that adds a subdirectory to the URI of gRPC requests.
/// </summary>
public class SubdirectoryHandler : DelegatingHandler
{
    private readonly string _subdirectory;

    public SubdirectoryHandler(HttpMessageHandler innerHandler, string subdirectory)
        : base(innerHandler)
    {
        _subdirectory = subdirectory;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uri? old = request.RequestUri;

        string? url = $"{old.Scheme}://{old.Host}:{old.Port}";
        url += $"{_subdirectory}{request.RequestUri.AbsolutePath}";
        request.RequestUri = new Uri(url, UriKind.Absolute);

        Console.WriteLine(request.RequestUri);

        var response = base.SendAsync(request, cancellationToken);

        return response;
    }
}