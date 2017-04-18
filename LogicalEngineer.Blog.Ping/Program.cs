/*
   The MIT License (MIT)
   Copyright (c) 2015 Logical Engineer

   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, 
   including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished 
   to do so, subject to the following conditions:

   The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
   THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace LogicalEngineer.Blog.Ping
{
    class Program
    {
        private const Int32 Tries = 6;
        private List<PingReply> Responses = new List<PingReply>();
        private IPAddress IPAddress;
        private Boolean HasPrintedStats = false;

        static void Main(string[] args)
        {
            var program = new Program();

            Console.CancelKeyPress += delegate
            {
                //When the user presses CTRL + C, we will print the statistics we have and then stop.
                program.PrintStatistics();
            };

            if (args != null && args.Length == 1)
                program.Run(args[0]);
            else
                Console.WriteLine("LogicalBit.Ping.exe ipOrHostName");
        }

        public void Run(String ipOrHostName)
        {
            var pingTest = new PingTest();

            var response = pingTest.FindIPAddress(ipOrHostName).Result;

            if (response.IsSuccessful)
            {
                this.IPAddress = response.IPAddress;
                Console.WriteLine(String.Format("Pinging {0} [{1}] with 32 bytes of data:", ipOrHostName, response.IPAddress.ToString()));

                for (var i = 0; i < Tries; i++)
                {
                    var pingResponse = pingTest.Ping(response.IPAddress).Result;
                    this.Responses.Add(pingResponse);

                    if (pingResponse.Status == IPStatus.Success)
                    {
                        Console.WriteLine(String.Format(
                                  "Reply from {0}: bytes={1} time={2}ms TTL={3}"
                                , pingResponse.Address.ToString()
                                , pingResponse.Buffer.Length
                                , pingResponse.RoundtripTime
                                , pingResponse.Options.Ttl
                        ));
                    }
                    else
                    {
                        Console.WriteLine(pingResponse.Status.ToString());
                    }

                    Thread.Sleep(500);
                }

                PrintStatistics();
            }
        }

        /// <summary>
        /// Aggregate and print high level detail about the PING test.
        /// </summary>
        public void PrintStatistics()
        {
            if (this.HasPrintedStats)
                return;

            var successPacketCount = this.Responses.Where(x => x.Status == IPStatus.Success).Count();
            var failurePacketCount = this.Responses.Where(x => x.Status != IPStatus.Success).Count();

            Console.WriteLine(String.Empty);
            Console.WriteLine(String.Format("Ping statistics for {0}:", this.IPAddress.ToString()));

            Console.WriteLine(String.Format(
                 "\tPackets: Sent = {0}, Received = {1}, Lost = {2} ({3}% loss)"
                , this.Responses.Count
                , successPacketCount
                , failurePacketCount
                , failurePacketCount == 0 ? 0M : (((failurePacketCount - successPacketCount) / failurePacketCount) * 100M)
            ));

            Console.WriteLine(String.Empty);
            Console.WriteLine("Approximate round trip times in milli-seconds:");

            Console.WriteLine(String.Format(
                 "\tMinimum = {0}ms, Maximum = {1}ms, Average = {2}ms"
                , this.Responses.Min(x => x.RoundtripTime)
                , this.Responses.Max(x => x.RoundtripTime)
                , this.Responses.Average(x => x.RoundtripTime).ToString("#0.00")
            ));


            this.HasPrintedStats = true;
        }
    }
}