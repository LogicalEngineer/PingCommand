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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LogicalEngineer.Blog.Ping
{
    public class PingTest
    {
        private readonly System.Net.NetworkInformation.Ping _ping;

        public PingTest()
        {
            this._ping = new System.Net.NetworkInformation.Ping();
        }

        public Task<PingReply> Ping(IPAddress ipAddress)
        {
            return this._ping.SendPingAsync(ipAddress);
        }

        /// <summary>
        /// The response we sind from the FindIPAddress. It'll
        /// contain the IPAddress and any error info.
        /// </summary>
        public class IPAddressResponse
        {
            public IPAddress IPAddress { get; set; }
            public Boolean IsSuccessful { get { return this.IPAddress != null; } }
            public String ErrorMessage { get; set; }
        }


        public async Task<IPAddressResponse> FindIPAddress(String hostName)
        {
            var response = new IPAddressResponse();

            if (!String.IsNullOrWhiteSpace(hostName))
            {
                try
                {
                    //IPAddress.Parse method is technically faster than the DNS.GetHostAddresses, but can't resolve host names.
                    //However, the speed improvement you get out of IPAddress.Parse will not be worth the extra code to maintain unless
                    //you're calling this in a tight loop in a critical piece of code. If you do, you can simply write a if statement around
                    //this section of code checking for an IP address and calling IPAddress.Parse if the user pased an IP Address instead of a hostname

                    //The Dns.GetHostAddresses relies on a DNS server to provide it the information, so it can be somewhat slow compared to conventional parsing
                    //of IP Addresses. However, it's needed to resolve the Hostnames.
                    var addresslist = await Dns.GetHostAddressesAsync(hostName);

                    //There can be multiple ip addresses (think of load balancing)
                    if (addresslist.Length > 0)
                        response.IPAddress = addresslist[0];
                }
                //The length of hostNameOrAddress is greater than 255 characters.
                catch (ArgumentOutOfRangeException argumentRangeException)
                {
                    response.ErrorMessage = argumentRangeException.Message;
                }
                //An error is encountered when resolving the hostName.
                catch (SocketException socketException)
                {
                    response.ErrorMessage = socketException.Message;
                }
                //hostName is an invalid address.
                catch (ArgumentException argumentException)
                {
                    response.ErrorMessage = argumentException.ParamName;
                }
            }
            else
            {
                response.ErrorMessage = "No Hostname or IP Address provided.";
            }

            return response;
        }
    }
}
