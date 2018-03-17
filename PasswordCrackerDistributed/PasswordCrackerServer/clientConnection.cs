using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    class clientConnection
    {
        public TcpClient ConnectionSocket { get; set; }
        public string Chunk { get; set; }
        public string Pass { get; set; }


        public clientConnection(TcpClient connectionSocket, string chunk, string pass)
        {
            ConnectionSocket = connectionSocket;
            Chunk = chunk;
            Pass = pass;
        }

        public async Task<string> SendToClient()
        {
            Stream ns = ConnectionSocket.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns)
            {
                AutoFlush = true
            };

            sw.WriteLine($"DPCP 1.0\n{Pass}\n{Chunk}\n\n");
            //sw.WriteLine(pass);
            //sw.WriteLine(chunk);
            
            return sr.ReadLine();
        }
    }
}
