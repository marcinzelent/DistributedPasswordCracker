using PassCrackerClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PassCrackerClient
{
    class Program
    {
        private static StreamReader sr;
        private static StreamWriter sw;
        static void Main(string[] args)
        {
            TcpClient clientSocket = new TcpClient("192.168.3.163", 6789);
            NetworkStream ns = clientSocket.GetStream();
            sr = new StreamReader(ns);
            sw = new StreamWriter(ns);
            sw.AutoFlush = true;

            string data = GetData();
            string dictionary = ParseData(data);
            string[,] splitDictionary = SplitDictionary(dictionary);

            List<UserInfoClearText> result = new List<UserInfoClearText>();
            for(int i = 0; i < 5; i++)
            {
                int rowLength = splitDictionary.GetLength(1);
                string[] chunk = new string[rowLength];
                for (int j = 0; j < rowLength; j++)
                    chunk[j] = splitDictionary[i, j];
                result = DecryptPassword(chunk);
            }
            if (result.Count != 0)
            {
                sw.WriteLine(result[0].ToString());
            }
            else
            {
                sw.WriteLine("Nothing Found!");
            }
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
            string[,] splitDictionary = new string[5,dicWordsLength/5];
            int offset = 0;
            int j = 0;

            for (int i = 0; i < 5; i++)
            {
                while (j < dicWordsLength/5)
                {
                    splitDictionary[i, j] = dicWords[j + offset];
                    j++;
                }
                offset += dicWordsLength/5;
                j = 0;
            }

            return splitDictionary;
        }

        private static List<UserInfoClearText> DecryptPassword(string[] dictionary)
        {
            Cracking cracker = new Cracking();
            var result = cracker.RunCracking(dictionary);

            return result;
        }
    }
}
