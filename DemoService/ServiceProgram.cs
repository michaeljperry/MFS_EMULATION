using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace DemoService
{
    class ServiceProgram
    {
        static void Main(string[] args)
        {
            try
            {
                // Generally ok to use ports between 49152 and 65535
                // should check to make sure port isn't in use
                int port = 50000;
                AsyncService service = new AsyncService(port);
                service.Run();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }

    public class AsyncService
    {
        private IPAddress ipAddress;
        private int port;

        public AsyncService(int port)
        {
            this.port = port;
            string hostName = Dns.GetHostName();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName);
            this.ipAddress = IPAddress.Any;
            //for(int i = 0; i < ipHostInfo.AddressList.Length; ++i)
            //{
            //    if(ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        this.ipAddress = ipHostInfo.AddressList[i];
            //        break;
            //    }
            //}
            if (this.ipAddress == null)
                throw new Exception("No IPv4 address for server");
        }

        public async void Run( )
        {
            TcpListener listener = new TcpListener(this.ipAddress, this.port);
            listener.Start();
            Console.WriteLine("Array Min and Avg service is now running");
            Console.WriteLine("on IP " + this.ipAddress + " on port " + this.port);
            Console.WriteLine("Hit <enter> to stop service\n");
            while(true)
            {
                try
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Task t = Process(tcpClient);
                    await t;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task Process(TcpClient tcpClient)
        {
            string clientEndPoint =
    tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Received connection request from "
              + clientEndPoint);
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
                while (true)
                {
                    string request = await reader.ReadLineAsync();
                    if (request != null)
                    {
                        Console.WriteLine("Received service request: " + request);
                        string response = Response(request);
                        Console.WriteLine("Computed response is: " + response + "\n");
                        await writer.WriteLineAsync(response);
                    }
                    else
                    {
                        Console.WriteLine("Request is null.");
                        break; // Client closed connection
                    }
                        
                }
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
        }

        private static string Response(string request)
        {
            string testResponse = "C-CONSYSEWM1                      AA00000000000000000015    SYBE";
            //string[] pairs = request.Split('&');
            //string methodName = pairs[0].Split('=')[1];
            //string valueString = pairs[1].Split('=')[1];
            //string[] values = valueString.Split(' ');
            //double[] vals = new double[values.Length];
            //for (int i = 0; i < values.Length; ++i)
            //    vals[i] = double.Parse(values[i]);
            //string response = "";
            //if (methodName == "average") response += Average(vals);
            //else if (methodName == "minimum") response += Minimum(vals);
            //else response += "BAD methodName: " + methodName;
            //int delay = ((int)vals[0]) * 1000; // Dummy delay
            //System.Threading.Thread.Sleep(delay);
            return testResponse;
        }

        private static double Average(double[] vals)
        {
            double sum = 0.0;
            for (int i = 0; i < vals.Length; ++i)
                sum += vals[i];
            return sum / vals.Length;
        }

        private static double Minimum(double[] vals)
        {
            double min = vals[0]; ;
            for (int i = 0; i < vals.Length; ++i)
                if (vals[i] < min) min = vals[i];
            return min;
        }


    }
}
