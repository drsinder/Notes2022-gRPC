using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.Text;

namespace Notes2022MauiLib
{
    // All the code in this file is included in all platforms.
    public class MauiFileActions
    {
        public async Task SaveFileToClipBoard(byte[] data)
        {
            string textData = Encoding.Default.GetString(data);

            await Clipboard.Default.SetTextAsync(textData);
        }


        public async Task SaveFileToClipBoard(string data)
        {
            await Clipboard.Default.SetTextAsync(data);
        }

        public async Task<string> ReadClipboard()
        {
            if (Clipboard.Default.HasText)
            {
                return await Clipboard.Default.GetTextAsync();
            }
            else
                return null;
        }


    }

}

