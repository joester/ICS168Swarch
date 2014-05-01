using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace SwarchServer
{
    class Server
    {
        protected int maxPlayers;
        protected int numberOfClients;
        protected static TcpListener listener;
        protected static Socket[] socArray;
        protected static Client[] clientArray;

        protected static Stopwatch uniClock;

        protected static Client client1;
        protected static Client client2;
        protected static Client client3;
        protected static Client client4;

        protected static bool playing = false;

        public Server()
        {
            maxPlayers = 4;
            numberOfClients = 0;
            listener = new TcpListener(4185);
            socArray = new Socket[maxPlayers];
            clientArray = new Client[maxPlayers];
        }

        public void Listen()
        {
            listener.Start();

            while (numberOfClients < maxPlayers)
            {
                //accept an incoming request to make a connection then return 
                //the socket(endpoint) associate with the connection 
                Socket soc = listener.AcceptSocket();
                socArray[numberOfClients] = soc;


                Console.WriteLine("Connected: {0}", soc.RemoteEndPoint);//print connection details
                try
                {
                    NetworkStream nws = new NetworkStream(soc);
                    StreamReader sr = new StreamReader(nws);//return the stream to read from
                    StreamWriter sw = new StreamWriter(nws); //establish a stream to read to
                    sw.AutoFlush = true;//enable automatic flushing, flush thte wrtie stream after every write command, no need to send buffered data

                    clientArray[numberOfClients] = new Client(nws, sr, sw, ++numberOfClients);

                    Console.WriteLine("Client " + numberOfClients + " has connected");
                    sw.WriteLine("client\\" + numberOfClients);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

        public class ServerLoop
        {

            Thread thread1;
            Thread thread2;

            bool client1Ready;
            bool client2Ready;

            public ServerLoop(Client[] clients)
            {
                client1 = clients[0];
                client2 = clients[1];

                thread1 = client1.thread;
                thread2 = client2.thread;

                thread1.Start();
                thread2.Start();

                uniClock.Start();

                client1Ready = false;
                client2Ready = false;
            }



            public void loop()
            {

                while (true)
                {
                    if (client1Ready && client2Ready)
                    {

                        client1Ready = false;
                        client2Ready = false;

                        playing = true;

                    }

                    if (client1.commandQueue.Count > 0)
                    {
                        string command = client1.commandQueue.Dequeue();

                        string[] tokens = command.Split(new string[] { "\\" }, StringSplitOptions.None);

                        if (tokens[0].Equals("play"))
                        {
                            client1Ready = true;
                        }

                        else if (tokens[0].Equals("score"))
                        {
                            client1.sw.WriteLine("score\\" + tokens[1]);
                            client2.sw.WriteLine("score\\" + tokens[1]);
                        }

                        else if (tokens[0].Equals("lag"))
                        {
                            DateTime dt = NTPTime.getNTPTime(ref uniClock);
                            dt.AddMinutes(uniClock.Elapsed.Minutes);
                            dt.AddSeconds(uniClock.Elapsed.Seconds);
                            dt.AddMilliseconds(uniClock.ElapsedMilliseconds);

                            long ticks = dt.Ticks;

                            client1.sw.WriteLine("lag\\" + ticks);
                        }
                        else
                        {
                            //do nothing
                        }

                    }

                    if (client2.commandQueue.Count > 0)
                    {
                        string command = client2.commandQueue.Dequeue();

                        string[] tokens = command.Split(new string[] { "\\" }, StringSplitOptions.None);

                        if (tokens[0].Equals("play"))
                        {
                            client2Ready = true;
                        }

                        else if (tokens[0].Equals("score"))
                        {
                            client1.sw.WriteLine("score\\" + tokens[1]);
                            client2.sw.WriteLine("score\\" + tokens[1]);
                        }
                        else if (tokens[0].Equals("lag"))
                        {
                            DateTime dt = NTPTime.getNTPTime(ref uniClock);
                            dt.AddMinutes(uniClock.Elapsed.Minutes);
                            dt.AddSeconds(uniClock.Elapsed.Seconds);
                            dt.AddMilliseconds(uniClock.ElapsedMilliseconds);

                            long ticks = dt.Ticks;

                            client2.sw.WriteLine("lag\\" + ticks);
                        }
                        else
                        {
                            //do nothing
                        }
                    }
                }
            }
        }


        public class Client
        {
            public NetworkStream nws;
            public StreamReader sr;
            public StreamWriter sw;
            public Queue<string> commandQueue;
            public Thread thread;

            public int clientNumber;


            public Client(NetworkStream nws, StreamReader sr, StreamWriter sw, int clientNumber)
            {
                this.nws = nws;
                this.sr = sr;
                this.sw = sw;
                this.clientNumber = clientNumber;

                thread = new Thread(new ThreadStart(this.Service));
                commandQueue = new Queue<string>();
            }

            public void Service()
            {
                try
                {
                    while (true)
                    {
                        string data = sr.ReadLine();

                        lock (commandQueue)
                        {
                            commandQueue.Enqueue(data);
                        }
                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
