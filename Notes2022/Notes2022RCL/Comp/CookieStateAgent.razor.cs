using Microsoft.JSInterop;
using Notes2022.Proto;
using System.Text.Json;
using Grpc.Core;
using System.Timers;
using Syncfusion.Licensing;

namespace Notes2022RCL.Comp
{
    public partial class CookieStateAgent
    {
        /// <summary>
        /// The saved login value used while updating cookies
        /// </summary>
        private LoginReply? savedLoginValue; // used while updating cookies
        /// <summary>
        /// The module for calling javascript
        /// </summary>
        private IJSObjectReference? module; // for calling javascript
        private System.Timers.Timer pinger { get; set; }


#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        async ValueTask IAsyncDisposable.DisposeAsync()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

        {
            if (module is not null)
            {
                await module.DisposeAsync();
                module = null;
            }
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            pinger = new(60000); // ping server every 60 seconds to keep it alive
            pinger.Elapsed += Ping;
            pinger.Enabled = true;
            pinger.Start();
            AString key = await Client.GetTextFileAsync(new AString()
            { Val = "syncfusionkey.rsghjjsrsrj43632353" });

            SyncfusionLicenseProvider.RegisterLicense(key.Val);


            if (Globals.IsMaui)
                return;

            AString cookiename = await Client.GetTextFileAsync(new AString()
            { Val = "CookieName" });
            Globals.Cookie = cookiename.Val;
            // JS injected in .razor file - make sure the cookie.js is loaded
            if (module is null)
                module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");
            //try
            //{
            //    await Client.SpinUpAsync(new NoRequest());
            //}
            //catch (Exception ex)
            //{ }
            if (myState.IsAuthenticated) // nothing more to do here!
                return;
            savedLoginValue = myState.LoginReply; // should be null
            try
            {
                await GetLoginReplyAsync(); // try to get a cookie to authenticate
            }
            catch (Exception)
            {
            }
        }

        protected void Ping(Object source, ElapsedEventArgs e)
        {
            //if (Globals.IsMaui)
            //    return;

            _ = Client.NoOpAsync(new NoRequest()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Try to get login cookie
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task GetLoginReplyAsync()
        {
            if (Globals.IsMaui)
                return;

            try
            {
                if (module is null)
                    module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");
                string? cookie = await ReadCookie(Globals.Cookie);
                if (!string.IsNullOrEmpty(cookie))
                {
                    // found a cookie!
                    savedLoginValue = JsonSerializer.Deserialize<LoginReply>(cookie);
                    savedLogin = savedLoginValue; // save the value - login
                    if (Globals.NavMenu != null)
                    {
                        await Globals.NavMenu.Reload();
                    }

                    if (Globals.LoginDisplay != null)
                    {
                        Globals.LoginDisplay.Reload();
                    }

                    NotifyStateChanged(); // notify subscribers
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Read a cookie
        /// </summary>
        /// <param name = "cookieName">cookie name</param>
        /// <returns>needs to be deserialized)</returns>
        public async Task<string?> ReadCookie(string cookieName)
        {
            if (Globals.IsMaui)
                return null;

            if (module is not null)
            {
                try
                {
                    return Globals.Base64Decode(await module.InvokeAsync<string>("ReadCookie", cookieName));
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Write a Cookie
        /// </summary>
        /// <param name = "cookieName">Name of the cookie</param>
        /// <param name = "newCookie">Serialized cookie</param>
        /// <param name = "hours">expiry</param>
        public async Task WriteCookie(string cookieName, string newCookie, int hours)
        {
            if (Globals.IsMaui)
                return;

            if (module is not null)
            {
                try
                {
                    await module.InvokeAsync<string>("CreateCookie", cookieName, Globals.Base64Encode(newCookie), hours);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Dealing with login related info
        /// </summary>
        private LoginReply? savedLogin;
        /// <summary>
        /// Gets or sets the login reply.  Setting also notifies subsrcibers
        /// </summary>
        /// <value>
        /// The LoginReply - the current state of login.
        /// </value>
        public LoginReply? LoginReply
        {
            get
            {
                return savedLogin;
            }

            set
            {
                savedLogin = value;

                if (Globals.IsMaui)
                    return;

                // now save login cookie state
                if (savedLogin != null)
                {
                    WriteCookie(Globals.Cookie, JsonSerializer.Serialize(savedLogin), savedLogin.Hours).GetAwaiter();
                }
                else
                {
                    WriteCookie(Globals.Cookie, JsonSerializer.Serialize(new LoginReply()), 0).GetAwaiter();
                }

                NotifyStateChanged(); // notify subscribers
            }
        }

        /// <summary>
        /// Occurs when Login state changes.
        /// </summary>
        public event System.Action? OnChange;
        /// <summary>
        /// Notifies subscribers of login state change.
        /// </summary>
        private void NotifyStateChanged()
        {
            if (Globals.IsMaui)
                return;

            OnChange?.Invoke();
        }

        /// <summary>
        /// Check if user is authenticated - Login reply is not null and status == 200
        /// </summary>
        /// <value><c>true</c> if this instance is authenticated; otherwise, <c>false</c>.</value>
        public bool IsAuthenticated
        {
            get
            {
                return (LoginReply is not null) && LoginReply.Status == 200;
            }
        }

        /// <summary>
        /// Is user in Admin role
        /// </summary>
        /// <value><c>true</c> if this instance is admin; otherwise, <c>false</c>.</value>
        public bool IsAdmin
        {
            get
            {
                if (LoginReply is null || LoginReply.Status != 200)
                    return false;
                return UserInfo is not null && UserInfo.IsAdmin;
            }
        }

        /// <summary>
        /// Is user in User role
        /// </summary>
        /// <value><c>true</c> if this instance is user; otherwise, <c>false</c>.</value>
        public bool IsUser
        {
            get
            {
                if (LoginReply is null || LoginReply.Status != 200)
                    return false;
                return UserInfo is not null && UserInfo.IsUser;
            }
        }

        /// <summary>
        /// Get a Metadata/header for authetication to server in gRPC calls
        /// </summary>
        /// <value>The authentication header.</value>
        public Metadata AuthHeader
        {
            get
            {
                var headers = new Metadata();
                if (LoginReply is not null && LoginReply.Status == 200)
                    headers.Add("Authorization", $"Bearer {LoginReply.Jwt}");
                return headers;
            }
        }

        /// <summary>
        /// Get the decoded user info
        /// </summary>
        /// <value>The user information.</value>
        public UserInfo? UserInfo
        {
            get
            {
                if (LoginReply is not null && LoginReply.Status == 200)
                {
                    return LoginReply.Info;
                }

                return null;
            }
        }

    }
}