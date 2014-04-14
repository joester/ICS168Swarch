using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project1
{
    public class PongServer
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Listen();
        }
    }
}
