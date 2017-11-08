
using System;
using System.IO;
using System.Text;

using Dicom.Log;
using Dicom.Network;

namespace Dicom.CFindSCP
{
    internal class Program
    {
        
        private static void Main(string[] args)
        {
            // preload dictionary to prevent timeouts
            var dict = DicomDictionary.Default;


            // start DICOM server on port from command line argument or 11112
            int tmp;
            var port = args != null && args.Length > 0 && int.TryParse(args[0], out tmp) ? tmp : 11113;
            Console.WriteLine($"Starting C-Store SCP server on port {port}");

            var server = DicomServer.Create<CFindSCP>(port);
            
            // end process
            Console.WriteLine("Press <return> to end...");
            Console.ReadLine();
        }

        
    }
}
