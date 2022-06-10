// ***********************************************************************
// Assembly         : Notes2022.Server
// Author           : Dale Sinder
// Created          : 04-19-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-05-2022
// ***********************************************************************
// <copyright file="Program.cs" company="Notes2022.Server">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Notes2022.Server.Data;
using Notes2022.Server.Services;
using Notes2022.Server;
using Microsoft.AspNetCore.Identity.UI.Services;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Notes2022.Server.Hubs;
using Microsoft.AspNetCore.ResponseCompression;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8601 // Possible null reference assignment.

string? sentry = configuration["Sentry:Flag"];
string? GrpcReflect = configuration["GrpcReflect"];
string? connectionString = configuration.GetConnectionString("DefaultConnection");

// User Sentry?
if (!string.IsNullOrEmpty(sentry) && sentry == "true")
    builder.WebHost.UseSentry();

// Our database
builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Hangfire services.  Uses same database.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

// For Identity.  Uses same database
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<NotesDbContext>()
    .AddDefaultTokenProviders();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Add Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidAudience = configuration["JWTAuth:ValidAudienceURL"],
        ValidIssuer = configuration["JWTAuth:ValidIssuerURL"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTAuth:SecretKey"]))
    };
});

// Configure Identity options.
builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Default Password settings.
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.User.RequireUniqueEmail = true;
});

// Add Email service
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Add Grpc
string? transcode = configuration["JsonTranscoding"];
if (!string.IsNullOrEmpty(transcode) && transcode == "true")
{
    builder.Services.AddGrpc()
            .AddServiceOptions<Notes2022Service>(options =>
            {
                options.MaxReceiveMessageSize = 50 * 1024 * 1024; // 50 MB
                options.MaxSendMessageSize = 50 * 1024 * 1024; // 50 MB
                if (!string.IsNullOrEmpty(configuration["GrpcLogging"]) && configuration["GrpcLogging"] == "true")
                    options.Interceptors.Add<ServerLoggingInterceptor>();
                options.Interceptors.Add<AuthInterceptor>();
            })
            .AddJsonTranscoding();
}
else
{
    builder.Services.AddGrpc()
            .AddServiceOptions<Notes2022Service>(options =>
            {
                options.MaxReceiveMessageSize = 50 * 1024 * 1024; // 50 MB
                options.MaxSendMessageSize = 50 * 1024 * 1024; // 50 MB
                if (!string.IsNullOrEmpty(configuration["GrpcLogging"]) && configuration["GrpcLogging"] == "true")
                    options.Interceptors.Add<ServerLoggingInterceptor>();
                options.Interceptors.Add<AuthInterceptor>();
            });
}

// GRPC Reflection?
if (!string.IsNullOrEmpty(GrpcReflect) && GrpcReflect == "true")
    builder.Services.AddGrpcReflection();

// Cross-site
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

//builder.Services.AddControllers();
//builder.Services.AddRazorPages();

// Set Globals from configuration
Globals.SendGridApiKey = configuration["SendGridApiKey"];
Globals.SendGridEmail = configuration["SendGridEmail"];
Globals.SendGridName = configuration["SendGridName"];
Globals.ImportRoot = configuration["ImportRoot"];
Globals.AppUrl = configuration["AppUrl"];
Globals.CookieName = configuration["CookieName"];
Globals.StartTime = DateTime.UtcNow;

try
{
    Globals.ErrorThreshold = long.Parse(configuration["GrpcErrorThreshold"]);
    Globals.WarnThreshold = long.Parse(configuration["GrpcWarnThreshold"]);

    Globals.ImportMailInterval = int.Parse(configuration["ImportMailInterval"]);
    Globals.UserDict = configuration["UserDict"] == "true";
}
catch { }


//builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

WebApplication? app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.

string? debug = configuration["Debug"];
bool Debug = false;
if (!string.IsNullOrEmpty(debug) && debug == "true")
    Debug = true;

if (app.Environment.IsDevelopment() || Debug)
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// GRPC Reflection?
if (!string.IsNullOrEmpty(GrpcReflect) && GrpcReflect == "true")
    app.MapGrpcReflectionService();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors();

app.MapRazorPages();
app.MapControllers();

app.MapHub<ChatHub>("/chathub");
app.MapHub<MasterHub>("/masterhub");

Globals.HangfireAddress = "/hangfire";

app.UseHangfireDashboard(Globals.HangfireAddress, new DashboardOptions
{
    Authorization = new List<IDashboardAuthorizationFilter> { new MyAuthorizationFilter() }
});

if (!string.IsNullOrEmpty(sentry) && sentry == "true")
    app.UseSentryTracing();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<Notes2022Service>().EnableGrpcWeb().RequireCors("AllowAll");
    //endpoints.MapHangfireDashboard(Globals.HangfireAddress, new DashboardOptions
    //{
    //    Authorization = new List<IDashboardAuthorizationFilter> { new MyAuthorizationFilter() }
    //});
    //endpoints.MapControllers();
});

app.MapFallbackToFile("index.html");

app.Run();

#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8604 // Possible null reference argument.


public class MyAuthorizationFilter : IDashboardAuthorizationFilter
{
    //[Authorize(Roles = "Admin")]
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}

