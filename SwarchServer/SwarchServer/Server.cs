using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Data.SQLite;

namespace SwarchServer
{
    class Server
    {
        protected int maxPlayers;
        protected int minPlayers;
        protected static int numberOfClients;
        protected static TcpListener listener;
        protected static Socket[] socArray;
        protected static Client[] clientArray;

        protected static Stopwatch uniClock;

        protected static Client client1;
        protected static Client client2;
        protected static Client client3;
        protected static Client client4;

        protected static bool playing = false;

        protected ServerLoop loop;

        public Thread listenerThead;

        public Server()
        {
            uniClock = new Stopwatch();

            maxPlayers = 4;
            minPlayers = 2;
            numberOfClients = 0;
            listener = new TcpListener(4185);
            socArray = new Socket[maxPlayers];
            clientArray = new Client[maxPlayers];

            listenerThead = new Thread(new ThreadStart(this.Listen));

            loop = new ServerLoop();
            loop.loopThread.Start();
        }

        public void Listen()
        {
            listener.Start();

            while (true)
            {
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

                        Client client = new Client(nws, sr, sw, numberOfClients + 1);

                        clientArray[numberOfClients] = client;

                        numberOfClients++;

                        loop.playerConnected();

                        Console.WriteLine("Client " + numberOfClients + " has connected");
                        sw.WriteLine("client\\" + client.clientNumber);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

        }

        public class ServerLoop
        {

            Thread thread1;
            Thread thread2;
            Thread thread3;
            Thread thread4;

            public Thread loopThread;

            public ServerLoop()
            {

                loopThread = new Thread(new ThreadStart(this.loop));
                
                uniClock.Start();
            }

            //happens when a player connects, need to update the client objects
            public void playerConnected()
            {
                switch(numberOfClients)
                {
                    case 1:
                        client1 = clientArray[0];
                        thread1 = client1.thread;
                        thread1.Start();
                        break;
                    case 2:
                        client2 = clientArray[1];
                        thread2 = client2.thread;
                        thread2.Start();
                        break;
                    case 3:
                        client3 = clientArray[2];
                        thread3 = client3.thread;
                        thread3.Start();
                        break;
                    case 4:
                        client4 = clientArray[3];
                        thread4 = client4.thread;
                        thread4.Start();
                        break;
                    
                }
            }

            //recursively check if all clients are ready
            public bool ClientsReady(Client[] clients, int currentClient) 
            {
                if (numberOfClients < 2)
                    return false;
                else if (currentClient == numberOfClients - 1)
                    return clients[currentClient].clientReady;
                else
                    return clients[currentClient].clientReady && ClientsReady(clients, currentClient++);
            }

            //server loop that checks messages from clients
            public void loop()
            {

                while (true)
                {
                    if (ClientsReady(clientArray, 0))
                    {

                        foreach (Client c in clientArray)
                        {
                            c.clientReady = false;
                            c.sw.WriteLine("start");
                        }

                        playing = true;


                    }

                    if (numberOfClients >= 2)
                    {

                        if (client1.commandQueue.Count > 0)
                        {
                            string command = client1.commandQueue.Dequeue();

                            string[] tokens = command.Split(new string[] { "\\" }, StringSplitOptions.None);

                            if (tokens[0].Equals("play"))
                            {
                                client1.clientReady = true;
                            }

                            else if (tokens[0].Equals("velocity"))
                            {
                                if (tokens[1].Equals("x"))
                                {
                                    client1.xVelocity = float.Parse(tokens[2]);
                                }
                                else
                                {
                                    client1.yVelocity = float.Parse(tokens[2]);
                                }

                                client2.sw.WriteLine("velocity\\1\\" + tokens[1] + "\\" + tokens[2]);

                                /*
                                client3.sw.WriteLine("Velocity\\1\\" + tokens[1] + "\\" + tokens[2]);
                                client4.sw.WriteLine("Velocity\\1\\" + tokens[1] + "\\" + tokens[2]);
                                 */
                            }

                            else if (tokens[0].Equals("position"))
                            {
                                if (tokens[1].Equals("x"))
                                {
                                    client1.whalePositionX = float.Parse(tokens[2]);
                                }
                                else if (tokens[1].Equals("y"))
                                {
                                    client1.whalePositionY = float.Parse(tokens[2]);
                                }

                                client2.sw.WriteLine("position\\1\\" + tokens[1] + "\\" + tokens[2]);
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
                                client2.clientReady = true;
                            }

                            else if (tokens[0].Equals("velocity"))
                            {
                                if (tokens[1].Equals("x"))
                                {
                                    client2.xVelocity = float.Parse(tokens[2]);
                                }
                                else
                                {
                                    client2.yVelocity = float.Parse(tokens[2]);
                                }

                                client1.sw.WriteLine("velocity\\2\\" + tokens[1] + "\\" + tokens[2]);

                                /*
                                client3.sw.WriteLine("Velocity\\1\\" + tokens[1] + "\\" + tokens[2]);
                                client4.sw.WriteLine("Velocity\\1\\" + tokens[1] + "\\" + tokens[2]);
                                 */
                            }

                            else if (tokens[0].Equals("position"))
                            {
                                if (tokens[1].Equals("x"))
                                {
                                    client2.whalePositionX = float.Parse(tokens[2]);
                                }
                                else if (tokens[1].Equals("y"))
                                {
                                    client2.whalePositionY = float.Parse(tokens[2]);
                                }

                                client1.sw.WriteLine("position\\2\\" + tokens[1] + "\\" + tokens[2]);
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
        }


        public class Client
        {
            public NetworkStream nws;
            public StreamReader sr;
            public StreamWriter sw;
            public Queue<string> commandQueue;
            public Thread thread;
            public Thread positionThread;

            public int clientNumber;
            public float whalePositionX = 0;
            public float whalePositionY = 0;
            public float xVelocity = 0;
            public float yVelocity = 0;

            public bool clientReady;

            public Client(NetworkStream nws, StreamReader sr, StreamWriter sw, int clientNumber)
            {
                this.nws = nws;
                this.sr = sr;
                this.sw = sw;
                this.clientNumber = clientNumber;

                thread = new Thread(new ThreadStart(this.Service));
                positionThread = new Thread(new ThreadStart(this.UpdatePosition));
                commandQueue = new Queue<string>();
            }

            public void UpdatePosition()
            {
                whalePositionX += xVelocity;
                whalePositionY += yVelocity;
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
