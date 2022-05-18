namespace TextToJson
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");


            Notes2022.Server.Importer imp = new Notes2022.Server.Importer();
            
            _ = imp.Import(args[0], args[1]).GetAwaiter().GetResult();

            //Thread.Sleep(100000);
        }
    }
}