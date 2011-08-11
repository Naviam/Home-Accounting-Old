using System;
using Naviam.WebUI.Helpers.Parsers;

namespace TestParsers
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = @"C:\MxDownload\mail.google.com";
            if (args.Length > 0)
            {
                fileName = args[0];
            }
            BelSwissParser parser = new BelSwissParser();
            bool myFile = parser.IsMyFile(fileName);
            Console.WriteLine(myFile);
            BelSwissStatement stat = parser.ParseFile(fileName);
        }
    }
}
