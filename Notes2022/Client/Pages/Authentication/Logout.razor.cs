using Notes2022.Proto;

namespace Notes2022.Client.Pages.Authentication
{
    public partial class Logout
    {
        protected override async Task OnInitializedAsync()
        {
            await AuthClient.LogoutAsync(new NoRequest(), myState.AuthHeader);
            myState.LoginReply = null;
            Navigation.NavigateTo("");
        }
    }
}