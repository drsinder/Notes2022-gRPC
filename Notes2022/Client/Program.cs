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
using Blazored.SessionStorage;
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
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddSingleton<CookieStateAgent>();       // for login state mgt = "myState" injection in _imports.razor

builder.Services.AddSyncfusionBlazor();     // options => { options.IgnoreScriptIsolation = true; });

SyncfusionLicenseProvider.RegisterLicense("NjQ2OTU1QDMyMzAyZTMxMmUzMFBsUmI0QUljc2lmOTlRTHEyVFZMMkZjUmhVU0FkWnBwbWRYRWtkcEQ0ZFU9");

Globals.IsMaui = false;

// Add my gRPC service so it can be injected.
builder.Services.AddSingleton(services =>
{
    HttpClient? httpClient = new(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
    string? baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
    GrpcChannel? channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 50 * 1024 * 1024 });

    //Grpc.Core.CallInvoker? invoker = channel.Intercept(new ClientLoggingInterceptor());

    Notes2022Server.Notes2022ServerClient Client = new Notes2022Server.Notes2022ServerClient(channel);

    return Client;
});

await builder.Build().RunAsync();
