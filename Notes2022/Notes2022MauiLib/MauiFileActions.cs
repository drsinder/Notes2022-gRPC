using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.Text;

namespace Notes2022MauiLib
{
    // All the code in this file is included in all platforms.
    public class MauiFileActions
    {
        /// <summary>
        /// Saves to file and clip board.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="data">The data.</param>
        /// <param name="cliptoo">if set to <c>true</c> clip saved too.</param>
        /// <returns>System.String.</returns>
        public async Task<string> SaveToFileAndClipBoard(string filename, byte[] data, bool cliptoo)
        {
            string textData = Encoding.Default.GetString(data);

            if (cliptoo)
                await Clipboard.Default.SetTextAsync(textData);

            string file = Path.Combine(FileSystem.Current.AppDataDirectory, filename);
            await File.WriteAllTextAsync(file, textData);

            return file;
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void DeleteFile(string filename)
        {
            string file = Path.Combine(FileSystem.Current.AppDataDirectory, filename);
            File.Delete(file);
        }

        /// <summary>
        /// Reads from file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>System.String.</returns>
        public async Task<string> ReadFromFile(string filename)
        {
            string file = Path.Combine(FileSystem.Current.AppDataDirectory, filename);

            if (File.Exists(file))
            {
                return await File.ReadAllTextAsync(file);
            }
            return null;
        }

        /// <summary>
        /// Reads the clipboard.
        /// </summary>
        /// <returns>System.String.</returns>
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

