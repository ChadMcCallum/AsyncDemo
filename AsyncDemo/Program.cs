using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Nito.AsyncEx;

namespace AsyncDemo
{
    class Program
    {
        private static FileStream stream;
        private static byte[] buffer;
        private static string downloadedString;

        static void Main(string[] args)
        {
            //sync
            var syncStream = new FileStream("file.txt", FileMode.Open);
            var syncBuffer = new byte[1024];
            var syncBytesRead = syncStream.Read(syncBuffer, 0, syncBuffer.Length);
            Console.WriteLine("Read " + syncBytesRead + " bytes syncronously");
            syncStream.Close();

            //Async Programming Model
            stream = new FileStream("file.txt", FileMode.Open, FileAccess.Read, FileShare.Read, 1024, true);
            buffer = new byte[1024];
            var asyncResult = stream.BeginRead(buffer, 0, buffer.Length, FinishedRead, null);
            //can also poll for status
            while(!asyncResult.IsCompleted)
            {
                Console.WriteLine("Waiting for BeginRead to complete");
            }
            stream.Close();

            //Event-based Async Pattern
            var client = new WebClient();
            client.DownloadStringCompleted += ClientOnDownloadStringCompleted;
            client.DownloadStringAsync(new Uri("http://www.rtigger.com"));
            
            while(string.IsNullOrEmpty(downloadedString))
            {
                Console.WriteLine("Waiting for DownloadString to complete");
                Thread.Sleep(100);
            }
            
            //Task Async Pattern
            var stream2 = new FileStream("file.txt", FileMode.Open, FileAccess.Read, FileShare.Read, 1024, true);
            var buffer2 = new byte[1024];
            var result = Task<int>.Factory.FromAsync(stream2.BeginRead, stream2.EndRead, buffer2, 0, buffer2.Length, null);
            //do other things while task is executing
            while(!result.IsCompleted)
            {
                Console.WriteLine("Waiting for Task to complete");
            }
            Console.WriteLine("Task finished, read " + result.Result + " bytes from file");

            //async pattern
            AsyncContext.Run(() => DownloadStringAsync("http://www.rtigger.com"));

            Console.ReadLine();
        }

        async private static void DownloadStringAsync(string uri)
        {
            var webClient = new WebClient();
            var task = webClient.DownloadStringTaskAsync("http://www.rtigger.com");
            //task has started at this point, but we can do other things
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Doing busy work while async operation executes");
                Thread.Sleep(100);
            }
            var result = await task;
            Console.WriteLine("Finished reading using async, read " + result.Length + " characters");
        }

        private static void ClientOnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs downloadStringCompletedEventArgs)
        {
            downloadedString = downloadStringCompletedEventArgs.Result;
            Console.WriteLine("Finished WebClient Download String, read " + downloadStringCompletedEventArgs.Result.Length + " characters");
        }

        private static void FinishedRead(IAsyncResult ar)
        {
            var bytesRead = stream.EndRead(ar);
            Console.WriteLine("Finished BeginRead, read " + bytesRead + " bytes");
        }
    }
}
