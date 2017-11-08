// Copyright (c) 2012-2016 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System;
using System.IO;
using System.Text;

using Dicom.Log;
using Dicom.Network;

namespace Dicom.CStoreSCP
{
    internal class Program
    {
        
        private static void Main(string[] args)
        {
            // preload dictionary to prevent timeouts
            var dict = DicomDictionary.Default;


            // start DICOM server on port from command line argument or 11112
            int tmp;
            var port = args != null && args.Length > 0 && int.TryParse(args[0], out tmp) ? tmp : 11112;
            Console.WriteLine($"Starting C-Store SCP server on port {port}");

            CStoreSCP.Initalize(@".\DICOM");
            var server = DicomServer.Create<CStoreSCP>(port);
            
            // end process
            Console.WriteLine("Press <return> to end...");
            Console.ReadLine();
        }

        
    }
}
