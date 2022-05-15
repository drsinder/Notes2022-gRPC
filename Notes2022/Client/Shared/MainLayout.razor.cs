//using Grpc.Core;
//using Microsoft.JSInterop;
//using Notes2022.Proto;
//using System.Text.Json;

//namespace Notes2022.Client.Shared
//{
//    /// <summary>
//    /// Extend the funtionality of the Layout to include login status and login cookie managment
//    /// </summary>
//    public partial class MainLayout
//    {
//        private LoginReply? savedLoginValue;        // used while updating cookies

//        private IJSObjectReference? module;

//        //private static bool Preloaded { get; set; } = false;

//        async ValueTask IAsyncDisposable.DisposeAsync()
//        {
//            if (module is not null)
//            {
//                await module.DisposeAsync();
//                module = null;
//            }
//        }

//        protected override async Task OnParametersSetAsync()
//        {
//            // JS injected in .razor file - make sure the cookie.js is loaded
//            if (module is null)
//                module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");

//            //if (!Preloaded)
//            //{
//            //    Preloaded = true;
//            //    try
//            //    {
//            //        await Client.SpinUpAsync(new NoRequest());
//            //    }
//            //    catch (Exception ex)
//            //    { }
//            //}

//            if (myState.IsAuthenticated)    // nothing more to do here!
//                return;

//            savedLoginValue = myState.LoginReply;   // should be null

//            try
//            {
//                await GetLoginReplyAsync();   // try to get a cookie to authenticate
//            }
//            catch (Exception ex)
//            {
//            }
//        }

//        /// <summary>
//        /// Try to get login cookie
//        /// </summary>
//        /// <returns></returns>
//        public async Task GetLoginReplyAsync()
//        {
//            try
//            {
//                if (module is null)
//                    module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");

//                string cookie = await ReadCookie(Globals.Cookie);
//                if (!string.IsNullOrEmpty(cookie))
//                {
//                    // found a cookie!
//                    savedLoginValue = JsonSerializer.Deserialize<LoginReply>(cookie);

//                    savedLogin = savedLoginValue;   // save the value - login

//                    if (Globals.NavMenu != null)
//                        await Globals.NavMenu.Reload();
//                    if (Globals.LoginDisplay != null)
//                        Globals.LoginDisplay.Reload();

//                    NotifyStateChanged();           // notify subscribers
//                }
//            }
//            catch (Exception ex)
//            {
//                string x = ex.Message;
//            }
//        }

//        /// <summary>
//        /// Read a cookie
//        /// </summary>
//        /// <param name="cookieName">cookie name</param>
//        /// <returns>needs to be deserialized)</returns>
//        public async Task<string> ReadCookie(string cookieName)
//        {
//            try
//            {
//                return Globals.Base64Decode(await module.InvokeAsync<string>("ReadCookie", cookieName));
//            }
//            catch (Exception ex)
//            {
//            }
//            return null;
//        }

//        /// <summary>
//        /// Write a Cookie
//        /// </summary>
//        /// <param name="cookieName">Name of the cookie</param>
//        /// <param name="newCookie">Serialized cookie</param>
//        /// <param name="hours">expiry</param>
//        /// <returns></returns>
//        public async Task WriteCookie(string cookieName, string newCookie, int hours)
//        {
//            try
//            {
//                await module.InvokeAsync<string>("CreateCookie", cookieName, Globals.Base64Encode(newCookie), hours);
//            }
//            catch (Exception ex)
//            {
//            }
//        }

//        // Globals.Base64Encode(JsonSerializer.Serialize(ar))
//        // JsonSerializer.Deserialize<LoginReply>(cookie)

//        /// <summary>
//        /// /////////////////////////////////////////////////////////////////////////////////////////
//        /// 
//        /// Dealing with login related info
//        /// 
//        /// ////////////////////////////////////////////////////////////////////////////////////////
//        /// </summary>

//        private LoginReply? savedLogin;

//        public LoginReply? LoginReply
//        {
//            get
//            {
//                return savedLogin;
//            }
//            set
//            {
//                savedLogin = value;

//                // now save login cookie state

//                if (savedLogin != null)
//                {
//                    WriteCookie(Globals.Cookie, JsonSerializer.Serialize(savedLogin), savedLogin.Hours).GetAwaiter();
//                }
//                else
//                {
//                    WriteCookie(Globals.Cookie, JsonSerializer.Serialize(new LoginReply()), 0).GetAwaiter();
//                }

//                NotifyStateChanged();   // notify subscribers
//            }
//        }

//        public event System.Action? OnChange;

//        private void NotifyStateChanged()
//        {
//            OnChange?.Invoke();
//        }

//        /// <summary>
//        /// Check if user is authenticated - Login replay is not null and status == 200
//        /// </summary>
//        public bool IsAuthenticated
//        {
//            get
//            {
//                return LoginReply != null && LoginReply.Status == 200;
//            }
//        }

//        /// <summary>
//        /// Is user in Admin role
//        /// </summary>
//        public bool IsAdmin
//        {
//            get
//            {
//                if (LoginReply == null || LoginReply.Status != 200)
//                    return false;
//                return UserInfo.IsAdmin;
//            }
//        }

//        /// <summary>
//        /// Is user in User role
//        /// </summary>
//        public bool IsUser
//        {
//            get
//            {
//                if (LoginReply == null || LoginReply.Status != 200)
//                    return false;
//                return UserInfo.IsUser;
//            }
//        }

//        /// <summary>
//        /// Get a Metadata/header for authetication to server in gRPC calls
//        /// </summary>
//        public Metadata AuthHeader
//        {
//            get
//            {
//                var headers = new Metadata();
//                if (LoginReply != null && LoginReply.Status == 200)
//                    headers.Add("Authorization", $"Bearer {LoginReply.Jwt}");
//                return headers;
//            }
//        }

//        /// <summary>
//        /// Get the decoded user info
//        /// </summary>
//        public UserInfo? UserInfo
//        {
//            get
//            {
//                if (LoginReply != null && LoginReply.Status == 200)
//                {

//                    return LoginReply.Info;
//                }

//                return null;
//            }
//        }
//    }
//}
