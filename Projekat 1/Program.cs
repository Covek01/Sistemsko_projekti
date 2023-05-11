using System;

namespace Projekat_1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HttpServer server = new HttpServer(".\\Resources\\");
            string[] prefixes = {"http://localhost:8085/"};
            server.StartServer(prefixes);
        }
    }
}