using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    public partial class License
    {
        private string text = string.Empty;
        protected override async Task OnParametersSetAsync()
        {
            text = (await Client.GetTextFileAsync(new AString()
            { Val = "license.html" })).Val;
        }
    }
}