using System;
using System.Diagnostics;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Data.SQLite;


namespace SwarchServer
{
    class Server
    {

		public static SQLiteConnection swarchDatabase;
		string name, password; 
		public static DataManager dm;


        protected int maxPlayers;
        protected int minPlayers;
        protected static int numberOfClients;
        protected static TcpListener listener;
        protected static Socket[] socArray;
        protected static Client[] clientArray;
		protected static List<String> loginNames;

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
			dm = new DataManager ();
			maxPlayers = 4; 
            minPlayers = 2;
            numberOfClients = 0;
            listener = new TcpListener(4185);
            socArray = new Socket[maxPlayers];
            clientArray = new Client[maxPlayers];
			loginNames = new List<string> ();



            listenerThead = new Thread(new ThreadStart(this.Listen));


            uniClock = new Stopwatch();


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
                        client.thread.Start();

                        clientArray[numberOfClients] = client;

                        numberOfClients++;

                        loop.playerConnected();

                        Console.WriteLine("Client " + client.clientNumber + " has connected");
                        client.sw.WriteLine("client\\" + client.clientNumber);

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
                        break;
                    case 2:
                        client2 = clientArray[1];
                        break;
                    case 3:
                        client3 = clientArray[2];
                        break;
                    case 4:
                        client4 = clientArray[3];
                        break;
                    
                }
            }

            //recursively check if all clients are ready
            public bool ClientsReady(Client[] clients, int currentClient) 
            {

                if (currentClient == 0)
                    return clients[currentClient].clientReady;
                else if (currentClient > 0)
                    return clients[currentClient].clientReady && ClientsReady(clientArray, currentClient - 1);

                return false;
              
            }

            //server loop that checks messages from clients
            public void loop()
            {

                while (true)
                {                 
                   try
                   {
                       if (ClientsReady(clientArray, numberOfClients-1) && numberOfClients > 1)
                       {

                           foreach (Client c in clientArray)
                           {
                               c.clientReady = false;
                               c.sw.WriteLine("start");
                           }

                           playing = true;
                       }

                       //check each client in the array
                       //if they have commands waiting in the queue, dequeue and execute that command
                       foreach (Client client in clientArray)
                       {
                           if (client.commandQueue.Count > 0)
                           {
                               string command = client.commandQueue.Dequeue();

                               string[] tokens = command.Split(new string[] { "\\" }, StringSplitOptions.None);

                               if (tokens[0].Equals("play"))
                               {
                                   client.clientReady = true;
                                   Console.WriteLine("client " + client.clientNumber + " is ready");

                                   //let all other players that have connected know that most recent client has connected
                                   //tell this client know about all the clients that have connected
                                   foreach (Client c in clientArray)
                                   {
                                       c.sw.WriteLine("connected\\" + client.clientNumber);

                                       if ((c.clientReady || playing) && !client.spawned)
                                       {
                                           if (c.clientNumber != client.clientNumber)
                                                 client.sw.WriteLine("connected\\" + c.clientNumber);

                                       }
                                   }

                                   client.spawned = true;
                               }

                               //


								if (tokens[0].Equals("userInfo"))
								{
									if(!loginNames.Contains(tokens[0]))
									{

										if (dm.existsInInfoTable(tokens[1]))
										{
											if (tokens[2].Equals(dm.getUserPassword(tokens[1])))
											{
												client.sw.WriteLine("loginSucceed\\" + tokens[1]);
												Console.WriteLine(tokens[1] + " has logged in");
											}
											else
											{
												client.sw.WriteLine("loginFail");
												Console.WriteLine(tokens[1] + " entered incorrect password");
											}
										}

										else
										{
											dm.insertIntoInfoTable(tokens[1], tokens[2]);
											loginNames.Add(tokens[1]);
											client.sw.WriteLine("loginSucceed\\" + tokens[1]);
											dm.printInfoTable();
										}


									}
									else
									{
										client.sw.WriteLine("exists\\" + tokens[1]);
									}


								}



                               

                                //if the velocity of a client has changed, they send it to the server
                                //the server broadcasts this velocity to all other clients
                               else if (tokens[0].Equals("velocity"))
                               {
                                   if (tokens[1].Equals("x"))
                                   {
                                       client.xVelocity = float.Parse(tokens[2]);
                                       client.yVelocity = 0;
                                   }
                                   else
                                   {
                                       client.yVelocity = float.Parse(tokens[2]);
                                       client.xVelocity = 0;
                                   }

                                   foreach (Client c in clientArray)
                                   {
                                       if (c.clientNumber != client.clientNumber)
                                           c.sw.WriteLine("velocity\\1\\" + client1.xVelocity + "\\" + client1.yVelocity);
                                   }

                               }

                               else if (tokens[0].Equals("position"))
                               {
                                   if (tokens[1].Equals("x"))
                                   {
                                       client.whalePositionX = float.Parse(tokens[2]);
                                   }
                                   else if (tokens[1].Equals("y"))
                                   {
                                       client.whalePositionY = float.Parse(tokens[2]);
                                   }

                                   foreach (Client c in clientArray)
                                   {
                                       if (c.clientNumber != client.clientNumber)
                                           c.sw.WriteLine("velocity\\1\\" + client.xVelocity + "\\" + client.yVelocity);
                                   }
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
                       }
                
                    }
                   catch (Exception e)
                   {
                       //Console.WriteLine(e.Message);
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

            public bool clientReady = false;
            public bool spawned = false;

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
