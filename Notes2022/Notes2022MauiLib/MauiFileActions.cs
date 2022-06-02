using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.Text;

namespace Notes2022MauiLib
{
    // All the code in this file is included in all platforms.
    public class MauiFileActions
    {
        public async Task<string> SaveToFileAndClipBoard(string filename, byte[] data, bool cliptoo)
        {
            string textData = Encoding.Default.GetString(data);

            if (cliptoo)
                await Clipboard.Default.SetTextAsync(textData);

            string file = Path.Combine(FileSystem.Current.AppDataDirectory, filename);

            await File.WriteAllTextAsync(file, textData);

            return file;
        }

        public void DeleteFile(string filename)
        {
            string file = Path.Combine(FileSystem.Current.AppDataDirectory, filename);
            File.Delete(file);
        }

        public async Task<string> ReadFromFile(string filename)
        {
            string file = Path.Combine(FileSystem.Current.AppDataDirectory, filename);

            if (File.Exists(file))
            {
                return await File.ReadAllTextAsync(file);
            }
            return null;
        }

        //public async Task SaveFileToClipBoard(string data)
        //{
        //    await Clipboard.Default.SetTextAsync(data);
        //}

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

