using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    class clientConnection
    {
        public TcpClient connectionSocket { get; set; }
        public string chunk { get; set; }

        public string pass { get; set; }


        public clientConnection(TcpClient connectionSocket, string chunk, string pass)
        {
            this.connectionSocket = connectionSocket;
            this.chunk = chunk;
            this.pass = pass;
        }

        public async Task<string> Doit()
        {
            Stream ns = connectionSocket.GetStream();
            StreamReader sr = new StreamReader(ns);

            StreamWriter sw = new StreamWriter(ns);
            sw.AutoFlush = true;

            sw.WriteLine($"DPCP 1.0\n{pass}\n{chunk}");
            //sw.WriteLine(pass);
            //sw.WriteLine(chunk);
            
            return sr.ReadLine();
        }
    }
}
