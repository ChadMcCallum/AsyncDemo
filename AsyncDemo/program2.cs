using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using Nito.AsyncEx;

namespace AsyncDemo
{
    public static class program2
    {
        private static FileStream stream;
        private static string downloadedString;

        public static void Main(string[] args)
        {
            //sync
            stream = new FileStream("file.txt", FileMode.Open);
            var buffer = new byte[1024];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            Console.WriteLine("Read " + bytesRead + " from file");
            stream.Close();

            //APM
            stream = new FileStream("file.txt", FileMode.Open, FileAccess.Read, FileShare.Read, 1024, true);
            var asyncResult = stream.BeginRead(buffer, 0, buffer.Length, FinishedRead, null);
            while (!asyncResult.IsCompleted)
            {
                Console.WriteLine("Waiting for begin read to finish");
            }

            //EAP
            var client = new WebClient();
            client.DownloadStringCompleted += OnDownloadStringCompleted;
            client.DownloadStringAsync(new Uri("http://rtigger.com"));
            while (string.IsNullOrEmpty(downloadedString))
            {
                Console.WriteLine("Waiting for download string to finish");
            }

            //task
            stream = new FileStream("file.txt", FileMode.Open, FileAccess.Read, FileShare.Read, 1024, true);
            var task = Task<int>.Factory.FromAsync(stream.BeginRead, stream.EndRead, buffer, 0, buffer.Length, null);
            while (!task.IsCompleted)
            {
                Console.WriteLine("Waiting for task to finish");
            }            
            Console.WriteLine("Task is finished, read " + task.Result + " bytes from file");

            AsyncContext.Run(() => DownloadStringAsync("http://www.rtigger.com"));
            
            Console.ReadLine();
        }

        async private static void DownloadStringAsync(string uri)
        {
            var client = new WebClient();
            Task<string> task = new Task<string>(() => "do stuff.");
            task.ConfigureAwait(false);
            task.Start();
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Doing busy work while async operation executes");
                Thread.Sleep(100);
            }
            var result = await task;
            Console.WriteLine("Task is finished, read " + result.Length + " characters");
        }

        private static void OnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            downloadedString = e.Result;
            Console.WriteLine("Finished web client download, read " + e.Result.Length + " characters");
        }

        private static void FinishedRead(IAsyncResult ar)
        {
            var bytesRead = stream.EndRead(ar);
            Console.WriteLine("Finished BeginRead, read " + bytesRead + " bytes from file");
        }
    }
}
