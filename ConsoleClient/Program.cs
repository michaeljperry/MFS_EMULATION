using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ConsoleClient
{
    class TestClient
    {
        private string server;
        private int port;
        private bool connected;
        private TcpClient client;
        private NetworkStream networkStream;
        private StreamReader reader;
        private StreamWriter writer;

        public TestClient(string server, int port)
        {
            this.server = server;
            this.port = port;
            this.connected = false;
        }

        public async Task<bool> Connect()
        {
            try
            {
                IPAddress ipAddress = null;
                IPHostEntry ipHostInfo = Dns.GetHostEntry(server);
                for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
                {
                    if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = ipHostInfo.AddressList[i];
                        break;
                    }
                }
                if (ipAddress == null)
                {
                    throw new Exception("No IPv4 address for server");
                }
                this.client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);
                networkStream = client.GetStream();
                writer = new StreamWriter(networkStream);
                reader = new StreamReader(networkStream);
                writer.AutoFlush = true;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public void Close()
        {
            client.Close();
            this.connected = false;
        }

        public async void Run()
        {
            Console.WriteLine("Sending Request . . .");
            Task<string> response = this.SendRequest();
            Console.WriteLine("Request sent, waiting for response . . .");
            await response;
            Console.WriteLine(response.Result);
            Console.WriteLine("Response received, closing connection . . .");
            this.Close();
        }

        public async Task<string> SendRequest()
        {
            string response;

            try
            {
                string requestData = "method=average&data=8 7 6&eor";
                await writer.WriteLineAsync(requestData);
                response = await reader.ReadLineAsync();               
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return response;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //192.168.86.32
            int port = 50000;
            var server = Dns.GetHostName();
            var client = new TestClient(server, port);
            Task<bool> connected = client.Connect();
            if (connected.Result)
            {
                client.Run();
            }
            Console.ReadKey();
        }
    }
}
