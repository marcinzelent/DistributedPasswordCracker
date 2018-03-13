using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PasswordCrackerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            List<UserInfo> userInfos = PasswordFileHandler.ReadPasswordFile("passwords.txt");
            List<clientConnection> clients = new List<clientConnection>();
            List<string> allchunks;

            string users = "";
            foreach (UserInfo u in userInfos)
            {
                users += u + "|";
            }
            if (
                File.Exists("chunk0.txt")
                &&
                File.Exists("chunk1.txt")
                &&
                File.Exists("chunk2.txt")
                &&
                File.Exists("chunk3.txt")
                &&
                File.Exists("chunk4.txt")
                
                )
            {
                string chunk1String = File.ReadAllText("chunk0.txt");
                string chunk2String = File.ReadAllText("chunk1.txt");
                string chunk3String = File.ReadAllText("chunk2.txt");
                string chunk4String = File.ReadAllText("chunk3.txt");
                string chunk5String = File.ReadAllText("chunk4.txt");


                allchunks = new List<string>()
                {
                    chunk1String,
                    chunk2String,
                    chunk3String,
                    chunk4String,
                    chunk5String
                };
            }
            else
            {
                List<string> words = new List<string>();

                using (FileStream fs = new FileStream("webster-dictionary.txt", FileMode.Open, FileAccess.Read))

                using (StreamReader dictionary = new StreamReader(fs))
                {
                    while (!dictionary.EndOfStream)
                    {
                        words.Add(dictionary.ReadLine());
                    }
                }


                List<string> chunk1 = new List<string>(words.GetRange(0, 65000));
                string chunk1String = "";
                foreach (string s1 in chunk1)
                {
                    chunk1String += s1 + "|";
                }
                List<string> chunk2 = new List<string>(words.GetRange(65000, 65000));
                string chunk2String = "";
                foreach (string s2 in chunk2)
                {
                    chunk2String += s2 + "|";
                }
                List<string> chunk3 = new List<string>(words.GetRange(130000, 65000));
                string chunk3String = "";
                foreach (string s3 in chunk3)
                {
                    chunk3String += s3 + "|";
                }
                List<string> chunk4 = new List<string>(words.GetRange(195000, 65000));
                string chunk4String = "";
                foreach (string s4 in chunk4)
                {
                    chunk4String += s4 + "|";
                }
                List<string> chunk5 = new List<string>(words.GetRange(260000, 51141));
                string chunk5String = "" + " ";
                foreach (string s5 in chunk5)
                {
                    chunk5String += s5 + "|";
                }
                allchunks = new List<string>()
                {
                    chunk1String,
                    chunk2String,
                    chunk3String,
                    chunk4String,
                    chunk5String
                };


                string path = "chunk";

                for (int i = 0; i < 5; i++)
                {
                    if (!File.Exists(path + i + ".txt"))
                    {
                        File.WriteAllText(path + i + ".txt", allchunks[i]);
                    }
                }
            }




            IPAddress ip = IPAddress.Any;
            TcpListener serversocket = new TcpListener(ip, 6789);
            
            serversocket.Start();
            Console.WriteLine("Server started");
            int counter = 0;
            while (true)
            {
                if (clients.Count == 5)
                {
                    string[] returnedResult = RunAsync(clients).Result;
                    foreach (string s in returnedResult)
                    {
                        Console.WriteLine(s);
                    }
                }
                else
                {
                    TcpClient connectionSocket = serversocket.AcceptTcpClient();
                    Console.WriteLine("Server activated");

                    clients.Add(new clientConnection(connectionSocket, allchunks[counter], users));
                    counter ++;
                }
            }
            serversocket.Stop();
        }

        public static async Task<string[]> RunAsync(List<clientConnection> clients)
        {


            var C1task = clients[0];
            var C2task = clients[1];
            var C3task = clients[2];
            var C4task = clients[3];
            var C5task = clients[4];
            

            string[] result = await Task.WhenAll(C1task.Doit(), C2task.Doit() ,C3task.Doit(), C4task.Doit(), C5task.Doit());

            /*
            var C1 = await C1task;
            var C2 = await C2task;
            var C3 = await C3task;
            var C4 = await C4task;
            var C5 = await C5task;
            */
            return result;
        }

    }
}
