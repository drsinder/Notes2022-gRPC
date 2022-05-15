using Microsoft.JSInterop;
using Notes2022.Proto;
using System.Text.Json;
using System.Web;

namespace Notes2022.Client.Comp
{

    /// <summary>
    /// This component is a helper for StateContainer and
    /// is added to the Layout to log user back in if they have the cookie.
    /// 
    /// It woud be better to do this in the StateContainer but the container is not
    /// a component so some things do not work there (JSInterop).
    /// </summary>
    public partial class CookieStateAgent
    {
        private LoginReply? savedLoginValue;

        protected override async Task OnParametersSetAsync()
        {
            if (myState.IsAuthenticated)    // nothing to do here!
                return;

            savedLoginValue = myState.LoginReply;

            try
            {
                //if (savedLogin == null && JS != null)
                {
                    await LoginReplyAsync();   // try to get a cookie to auth
                }
            }
            catch (Exception ex)
            {
            }
        }

        protected async Task LoginReplyAsync()
        {

            //if (savedLogin == null && JS != null)
            {
                try
                {
                    // JS injected in .razor file
                    IJSObjectReference module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");

                    string cookie = await ReadCookies(module);
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        // found a cookie
                        string json = HttpUtility.HtmlDecode(Globals.Base64Decode(cookie));
                        savedLoginValue = JsonSerializer.Deserialize<LoginReply>(json);

                        myState.LoginReply = savedLoginValue;
                    }

                    await module.DisposeAsync();
                }
                catch (Exception ex)
                {
                    string x = ex.Message;
                }
            }
        }

        private async Task<string> ReadCookies(IJSObjectReference module)
        {
            try
            {
                string cookie = await module.InvokeAsync<string>("ReadCookie", Globals.Cookie);
                return cookie;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}