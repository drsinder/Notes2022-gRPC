namespace Notes2022.TextToJson
{
    /// <summary>
    /// Program to convert exported notefiles in .txt to .json
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Converting...");

            Notes2022.TextToJson.Manager.Importer imp = new Notes2022.TextToJson.Manager.Importer();
            
            _ = imp.Import(args[0], args[1]).GetAwaiter().GetResult();

        }
    }
}