using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace Server
{
    internal class Program
    {
        static int id = 0;
        static void Answerer(NetworkStream stream, string user_question)
        {
            Console.Write($"{user_question}. | Відповідь: ");
            string answer = Console.ReadLine();
            byte[] answer_Bytes = Encoding.UTF8.GetBytes(answer);
            stream.Write(answer_Bytes, 0, answer_Bytes.Length);
        }
        static void BroadcastMessage()
        {
            UdpClient server = new UdpClient();
            server.EnableBroadcast = true;  
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Broadcast, 6001);

            Console.WriteLine($"[Broadcast] прийнято");
            while (true)
            {
                Thread.Sleep(3000);
                string announcement = "In building 1 there isn't water";
                byte[] announcement_Bytes = Encoding.UTF8.GetBytes(announcement);
                server.Send(announcement_Bytes, announcement_Bytes.Length, remoteEP);
                Thread.Sleep(2000);
            }
        }
        static void MulticastMessage(string groupIP, int port, string floor)
        {
            UdpClient server = new UdpClient();
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(groupIP), port);

            Console.WriteLine($"[Multicast] підключено до поверху {floor}");
            while (true)
            {
                Thread.Sleep(3000);
                string message = $"[Multicast {groupIP}] На поверсі є новина!";
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                server.Send(bytes, bytes.Length, remoteEP);
                Thread.Sleep(2000);
            }
        }
        static void ServerWork(TcpClient client)
        {
            while (true)
            {
                NetworkStream stream = client.GetStream();

                byte[] user_answer_Bytes = new byte[1024];
                int user_answer_Int = stream.Read(user_answer_Bytes, 0, user_answer_Bytes.Length);
                string user_answer = Encoding.UTF8.GetString(user_answer_Bytes, 0, user_answer_Int);

                if (user_answer == "1")
                {
                    byte[] user_question_Bytes = new byte[1024];
                    int user_question_Int = stream.Read(user_question_Bytes, 0, user_question_Bytes.Length);
                    string user_question = Encoding.UTF8.GetString(user_question_Bytes, 0, user_question_Int);
                    Thread thread1 = new Thread(() => Answerer(stream, user_question));
                    thread1.Start();
                }
                else if (user_answer == "2")
                {
                    Thread thread2 = new Thread(() => BroadcastMessage());
                    thread2.Start();
                }
                else if (user_answer == "3")
                {
                    byte[] floor_Bytes = new byte[1024];
                    int floor_Int = stream.Read(floor_Bytes, 0, floor_Bytes.Length);
                    string floor = Encoding.UTF8.GetString(floor_Bytes, 0, floor_Int);

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

                    int port = 7000;
                    Thread thread = new Thread(() => MulticastMessage(groupIP, port, floor));
                    thread.Start();
                }
            }
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Server online!");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine($"Client {++id} online");

                Thread thread = new Thread(() => ServerWork(client));
                thread.Start();
            }
        }
    }
}
