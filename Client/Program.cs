using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static Thread thread1;
        static bool flag_Broadcast = true;
        static bool flag_Help = false;
        static Thread thread2;
        static bool flag_Multicast = true;
        static bool flag_Help2 = false;
        static string lastGroupIP = "";
        static void GetMulticastMessage(string groupIP)
        {
            UdpClient client = new UdpClient();

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, 7000)); 

            client.JoinMulticastGroup(IPAddress.Parse(groupIP));
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine($"[Multicast] підключен...");

            while (flag_Multicast)
            {
                byte[] bytes = client.Receive(ref ep);
                string message = Encoding.UTF8.GetString(bytes);
                Console.WriteLine(message);
                Console.Write("-: ");
            }

            client.DropMulticastGroup(IPAddress.Parse(groupIP)); 
            client.Close();
        }
        static void GetBroadcastMessange()
        {
            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, 6001));
            IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("Broadcast підключен...");

            while (flag_Broadcast)
            {
                byte[] message_Bytes = client.Receive(ref clientEP);
                string message = Encoding.UTF8.GetString(message_Bytes);
                Console.WriteLine($"[Broadcast] {message}");
                Console.Write($"-: ");
            }
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();

            while (true)
            {
                Console.WriteLine($"1. Питання серверу");
                Console.WriteLine($"2. Слухати Broadcast");
                Console.WriteLine($"3. Приєднатись до Multicast-групи");
                Console.WriteLine($"4. Вийти");
                Console.Write($"-: ");
                string choice = Console.ReadLine();
                byte[] choice_Bytes = Encoding.UTF8.GetBytes(choice);
                stream.Write(choice_Bytes, 0, choice_Bytes.Length);


                if (choice == "1")
                {
                    if (thread1 != null && thread1.IsAlive)
                    {
                        flag_Broadcast = false;
                        thread1.Join();
                        thread1 = null;
                        flag_Help = false;
                    }
                    if (thread2 != null && thread2.IsAlive)
                    {
                        flag_Multicast = false;
                        thread2.Join();
                        thread2 = null;
                        flag_Help2 = false;
                    }
                    Console.Write($"Питання: ");
                    string question = Console.ReadLine();
                    byte[] question_Bytes = Encoding.UTF8.GetBytes(question);
                    stream.Write(question_Bytes, 0, question_Bytes.Length);
                    byte[] server_answer_Bytes = new byte[1024];
                    int server_answer_Int = stream.Read(server_answer_Bytes, 0, server_answer_Bytes.Length);
                    string server_answer = Encoding.UTF8.GetString(server_answer_Bytes, 0, server_answer_Int);
                    Console.WriteLine($"Відповідь: {server_answer}");
                    if (flag_Help2)
                    {
                        flag_Multicast = true;
                        thread2 = new Thread(() => GetMulticastMessage(lastGroupIP));
                        thread2.Start();
                    }
                    if (flag_Help)
                    {
                        flag_Broadcast = true;
                        thread1 = new Thread(() => GetBroadcastMessange());
                        thread1.Start();
                    }
                }
                else if (choice == "2")
                {
                    if (thread2 != null && thread2.IsAlive)
                    {
                        flag_Multicast = false;
                        thread2.Join();
                        thread2 = null;
                        flag_Help2 = false;
                    }
                    if ((thread1 == null || !thread1.IsAlive) && !flag_Help)
                    {
                        flag_Broadcast = true;
                        flag_Help = true;
                        thread1 = new Thread(() => GetBroadcastMessange());
                        thread1.Start();
                    }
                }
                else if (choice == "3")
                {
                    if (thread1 != null && thread1.IsAlive)
                    {
                        flag_Broadcast = false;
                        thread1.Join();
                        thread1 = null;
                        flag_Help = false;
                    }

                    Console.Write("Оберіть поверх (1–5): ");
                    string floor = Console.ReadLine();
                    byte[] floor_Bytes = Encoding.UTF8.GetBytes(floor);
                    stream.Write(floor_Bytes, 0, floor_Bytes.Length);

                    string groupIP;
                    if (floor == "1")
                    {
                        groupIP = "239.0.0.1";
                    }
                    else if (floor == "2")
                    {
                        groupIP = "239.0.0.2";
                    }
                    else if (floor == "3")
                    {
                        groupIP = "239.0.0.3";
                    }
                    else if (floor == "4")
                    {
                        groupIP = "239.0.0.4";
                    }
                    else if (floor == "5")
                    {
                        groupIP = "239.0.0.5";
                    }
                    else
                    {
                        groupIP = "239.0.0.1";
                    }
                    
                    lastGroupIP = groupIP;
                    flag_Multicast = true;
                    flag_Help2 = true;
                    thread2 = new Thread(() => GetMulticastMessage(groupIP));
                    thread2.Start();
                }
                else if (choice == "4")
                {
                    client.Close();
                    break;
                }
            }
        }
    }
}
