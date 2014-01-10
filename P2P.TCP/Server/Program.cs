using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P2P.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            try
            {

                server.Start();

                Console.ReadLine();

                server.Stop();

            }

            catch
            {

            }
        }
    }
}
