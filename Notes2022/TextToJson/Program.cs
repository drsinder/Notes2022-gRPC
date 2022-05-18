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

            string outfile = string.Empty;
            string infile = string.Empty;

            if (args.Count() > 0)
            {
                infile = args[0];
                if (!infile.ToLower().EndsWith(".txt"))
                {
                    infile += ".txt";
                }
                       
            }
            if (args.Count() > 1)
            {
                outfile = args[1];
            }
            else
            {
                outfile = infile.Replace(".txt", ".json");
            }

            Notes2022.TextToJson.Manager.Importer imp = new Notes2022.TextToJson.Manager.Importer();
            
            _ = imp.Import(infile, outfile).GetAwaiter().GetResult();

        }
    }
}