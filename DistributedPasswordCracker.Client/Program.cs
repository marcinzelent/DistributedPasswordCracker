using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using DistributedPasswordCracker.Client.Models;

namespace DistributedPasswordCracker.Client
{
    class Program
    {
        const int NumberOfTasks = 5;

        private static StreamReader sr;
        private static StreamWriter sw;

        static void Main(string[] args)
        {
            Console.Write("Connecting to server... ");
            TcpClient clientSocket = new TcpClient("127.0.0.1", 7777);
            NetworkStream ns = clientSocket.GetStream();
            Console.Write("OK\n");
            sr = new StreamReader(ns);
            sw = new StreamWriter(ns);
            sw.AutoFlush = true;

            Console.Write("Getting data from the server... ");
            string data = GetData();
            if (data != "") Console.Write("OK\n");

            Console.Write("Parsing data... ");
            string dictionary = ParseData(data);
            if (dictionary != "") Console.Write("OK\n");

            Console.Write("Splitting dictionary... ");
            string[,] splitDictionary = SplitDictionary(dictionary);
            if (splitDictionary[0,0] != "") Console.Write("OK\n");

            Console.WriteLine("Cracking...");
            List<Task<string>> tasks = new List<Task<string>>();
            for(int i = 0; i < NumberOfTasks; i++)
            {
                int rowLength = splitDictionary.GetLength(1);
                string[] chunk = new string[rowLength];
                for (int j = 0; j < rowLength; j++)
                    chunk[j] = splitDictionary[i, j];
                tasks.Add(Task.Run(()=>DecryptPassword(chunk)));
            }
            
            var result = Task.WhenAll(tasks).Result;
            if (result.Length != 0)
            {
                string output = "";
                for (int i = 0; i < result.Length; i++)
                    output += result[i];
                
                Console.Write("Sending results to server...");
                sw.WriteLine(output);
                Console.Write("OK\n");
            }
            else 
            {
                Console.Write("FAILED\n");
                Console.Write("Sending notification to server...");
                sw.WriteLine("No passwords cracked!");
                Console.Write("OK\n");
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string GetData()
        {
            string data = "";

            while (true)
            {
                string message = sr.ReadLine();
                if (message != "") data += message + "\n";
                else break;
            }

            return data;
        }

        private static string ParseData(string data)
        {
            var splitData = data.Split('\n');

            if (splitData[0] == "DPCP 1.0")
            {
                splitData[1] = splitData[1].Replace('|', '\n');
                File.WriteAllText("passwords.txt", splitData[1]);
                File.WriteAllText("dictionary.txt", splitData[2]);
            }

            return splitData[2];
        }

        private static string[,] SplitDictionary(string dictionary)
        {
            string[] dicWords = dictionary.Split('|');
            int dicWordsLength = dicWords.Length;
            string[,] splitDictionary = new string[NumberOfTasks,dicWordsLength/NumberOfTasks];
            int offset = 0;
            int j = 0;

            for (int i = 0; i < NumberOfTasks; i++)
            {
                while (j < dicWordsLength/NumberOfTasks)
                {
                    splitDictionary[i, j] = dicWords[j + offset];
                    j++;
                }
                offset += dicWordsLength/NumberOfTasks;
                j = 0;
            }

            return splitDictionary;
        }

        private static async Task<string> DecryptPassword(string[] dictionary)
        {
            Cracking cracker = new Cracking();
            var result = cracker.RunCracking(dictionary);

            return result;
        }
    }
}
