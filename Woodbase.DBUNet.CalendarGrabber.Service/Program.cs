using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Woodbase.DBUNet.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length == 0)
                args = new[] { "<connectionString>" };


            var service = new MatchGrabberService(args[0], args[1], args[2]);

            if (Environment.UserInteractive)
            {
                service.LoadGames(args[1], args[2]);
                Console.WriteLine("Press any key to stop program");
                Console.Read();
            }
            else
            {
                ServiceBase.Run(service);
            }

        }
    }
}
