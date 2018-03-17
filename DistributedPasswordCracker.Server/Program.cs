using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DistributedPasswordCracker.Server.Models;
using DistributedPasswordCracker.Server.Utilities;

namespace DistributedPasswordCracker.Server
{
    class Program
    {
        static int NumberOfClients = 3;

        static void Main(string[] args)
        {
            List<UserInfo> userInfos = PasswordFileHandler.ReadPasswordFile("passwords.txt");
            List<string> allChunks =  new List<string>();

            string users = "";
            foreach (UserInfo u in userInfos)
                users += u + "|";

            var dictionary = File.ReadAllText("dictionary.txt");
            dictionary = dictionary.Replace('\n', '|');
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

            TcpListener serverSocket = new TcpListener(IPAddress.Any, 7777);
            serverSocket.Start();

            Console.WriteLine("Server started.");
            
            List<Task<string>> tasks = new List<Task<string>>();

            for (int i = 0; i < NumberOfClients; i++)
            {
                var chunk = allChunks[i];
                tasks.Add(Task.Run(()=>RunClientAsync(serverSocket, users, chunk)));
            }
            string[] result = Task.WhenAll(tasks).Result;

            foreach (string s in result)
                Console.WriteLine(s);

            serverSocket.Stop();
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

        private async static Task<string> RunClientAsync(TcpListener serverSocket, string pass, string chunk)
        {
            TcpClient connectionSocket = serverSocket.AcceptTcpClient();;
            Stream ns = connectionSocket.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns)
            {
                AutoFlush = true
            };

            sw.WriteLine($"DPCP 1.0\n{pass}\n{chunk}\n\n");

            var result = await sr.ReadLineAsync();

            return result;
        }
    }
}