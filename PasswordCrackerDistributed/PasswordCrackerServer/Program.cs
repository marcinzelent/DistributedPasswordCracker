using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    class Program
    {
        static int NumberOfClients = 1;

        static void Main(string[] args)
        {
            List<UserInfo> userInfos = PasswordFileHandler.ReadPasswordFile("passwords.txt");
            List<clientConnection> clients = new List<clientConnection>();
            List<string> allChunks =  new List<string>();

            string users = "";
            foreach (UserInfo u in userInfos)
                users += u + "|";

            var dictionary = File.ReadAllText("webster-dictionary.txt");
            dictionary = dictionary.Replace("\r\n", "|");
            var splitDictionary = SplitDictionary(dictionary);
            for (int i = 0; i < splitDictionary.GetLength(0); i++)
            {
                StringBuilder chunk = new StringBuilder();
                for (int j = 0; j < splitDictionary.GetLength(1); j++)
                {
                    chunk.Append(splitDictionary[i, j]);
                    chunk.Append('|');
                }
                allChunks.Add(chunk.ToString());
            }

            IPAddress ip = IPAddress.Any;
            TcpListener serversocket = new TcpListener(ip, 6789);
            
            serversocket.Start();
            Console.WriteLine("Server started.");
            int counter = 0;
            while (true)
            {
                if (clients.Count == NumberOfClients)
                {
                    string[] returnedResult = RunAsync(clients).Result;
                    foreach (string s in returnedResult)
                        Console.WriteLine(s);
                }
                else
                {
                    TcpClient connectionSocket = serversocket.AcceptTcpClient();
                    Console.WriteLine("Client connected.");

                    clients.Add(new clientConnection(connectionSocket, allChunks[counter], users));
                    counter++;
                }
            }
            serversocket.Stop();
        }

        private static string[,] SplitDictionary(string dictionary)
        {
            string[] dicWords = dictionary.Split('|');
            int dicWordsLength = dicWords.Length;
            string[,] splitDictionary = new string[NumberOfClients, dicWordsLength / NumberOfClients];
            int offset = 0;
            int j = 0;

            for (int i = 0; i < NumberOfClients; i++)
            {
                while (j < dicWordsLength / NumberOfClients)
                {
                    splitDictionary[i, j] = dicWords[j + offset];
                    j++;
                }
                offset += dicWordsLength / NumberOfClients;
                j = 0;
            }

            return splitDictionary;
        }

        public static async Task<string[]> RunAsync(List<clientConnection> clients)
        {
            List<Task<string>> tasks = new List<Task<string>>();
            for (int i = 0; i < NumberOfClients; i++)
                tasks[i] = clients[i].SendToClient();

            string[] result = await Task.WhenAll(tasks);

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
