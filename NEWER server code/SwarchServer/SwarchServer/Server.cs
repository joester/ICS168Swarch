﻿using System;
using System.Drawing;
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
        protected static int winningWeight;
        protected static int pelletWeight;
        protected static Random rng;
        protected static int numberOfClients;
        protected static int numberOfPellets;
        protected static TcpListener listener;
        protected static Socket[] socArray;
        protected static Client[] clientArray;
        protected static Pellet[] pelletArray;
		protected static List<String> loginNames;
        

        protected static Stopwatch uniClock;

        protected static Client client1;
        protected static Client client2;
        protected static Client client3;
        protected static Client client4;

        protected static float TopBorderPosition;
        protected static float BottomBorderPosition;
        protected static float RightBorderPosition;
        protected static float LeftBorderPosition;

        protected static bool playing = false;

        protected ServerLoop loop;

        public Thread listenerThead;


        public Server()
        {
            
            dm = new DataManager();
            maxPlayers = 4;
            minPlayers = 2;
            winningWeight = 10;
            pelletWeight = 1;
            numberOfClients = 0;
            numberOfPellets = 5;

			loginNames = new List<string> ();
            TopBorderPosition = RightBorderPosition = 5.0f;
            BottomBorderPosition = LeftBorderPosition = -5.0f;

            rng = new Random();
            listener = new TcpListener(4185);
            socArray = new Socket[maxPlayers];
            clientArray = new Client[maxPlayers];
            pelletArray = new Pellet[numberOfPellets];

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
            public Thread collisionThread;

            public ServerLoop()
            {
                
                loopThread = new Thread(new ThreadStart(this.loop));
                collisionThread = new Thread(new ThreadStart(this.CheckCollisions));


                uniClock.Start();
            }

            //happens when a player connects, need to update the client objects
            public void playerConnected()
            {
                switch (numberOfClients)
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
                        if (ClientsReady(clientArray, numberOfClients - 1) && numberOfClients > 1)
                        {
                            //create all the pellets on the server
                            for (int i = 0; i < numberOfPellets; i++)
                            {
                                pelletArray[i] = new Pellet(i);
                            }

                            for (int i = 0; i < numberOfClients; i++)
                            {
                                Client c = clientArray[i];
                                c.clientReady = false;

                                //let all the clients know about each other
                                for (int j = 0; j < numberOfClients; j++ )
                                {
                                    Client c2 = clientArray[j];

                                    c2.sw.WriteLine("connected\\" + c.clientNumber);

                                }

                                //let all the clients know about the pellets
                                for (int j = 0; j < numberOfPellets; j++)
                                {
                                    Pellet pellet = pelletArray[j];
                                    c.sw.WriteLine("spawnPellet\\" + pellet.positionX + "\\" + pellet.positionY + "\\" + pellet.pelletID);
                                }

                                c.sw.WriteLine("start");
                            }

                            collisionThread.Start();
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
                                }


                                //
								else if (tokens[0].Equals("userInfo"))
                                {

									if(!loginNames.Contains(tokens[1]))
									{

										if (dm.existsInTable(tokens[1]))
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
											dm.insertIntoPlayer(tokens[1], tokens[2]);
											loginNames.Add(tokens[1]);
											client.sw.WriteLine("loginSucceed\\" + tokens[1]);
											dm.printTable();
										}


									}
									else{
										client.sw.WriteLine("alreadyLoggedIn\\" + tokens[1]);
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
                                        {
                                            c.sw.WriteLine("velocity\\" + client.clientNumber + "\\" + client.xVelocity + "\\" + client.yVelocity);
                                            
                                        }
                                    }

                                }

                                else if (tokens[0].Equals("position"))
                                {
                                    if (tokens[1].Equals("x"))
                                    {
                                        client.updatePosition(float.Parse(tokens[2]), client.whalePositionY);
                                    }
                                    else if (tokens[1].Equals("y"))
                                    {
                                        client.updatePosition(client.whalePositionX, float.Parse(tokens[2]));
                                    }

                                    foreach (Client c in clientArray)
                                    {
                                        if (c.clientNumber != client.clientNumber)
                                            c.sw.WriteLine("position\\" + client.clientNumber + "\\" + client.whalePositionX + "\\" + client.whalePositionY);
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

								else if (tokens[0].Equals("logout"))
								{
									Console.WriteLine("Name about to be removed {0}", tokens[1]);
									loginNames.Remove(tokens[1]);
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

            public void CheckCollisions()
            {
                while (true)
                {
                    for (int i = 0; i < numberOfClients; i++ )
                    {
                        Client c = clientArray[i];

                        foreach (Pellet p in pelletArray)
                        {
                            if (p.collided(c))
                            {
                                p.respawn();
                                c.weight += pelletWeight;
                                c.width += 0.1f;
                                c.height += 0.1f;
								c.score += 1;


                                for (int j = 0; j < numberOfClients; j++)
                                {
                                    Client c2 = clientArray[j];
                                    c2.sw.WriteLine("respawnPellet\\" + p.positionX + "\\" + p.positionY + "\\" + p.pelletID);

                                    //Console.WriteLine("Client " + c2.clientNumber + " was told to respawn pellet " + p.pelletID + " at position " + p.positionX + ", " + p.positionY);

                                    c2.sw.WriteLine("weight\\" + c.clientNumber + "\\" + pelletWeight);
									c2.sw.WriteLine ("score\\" + c.clientNumber + "\\" + c.score);
                                }
                            }
                        }

                        //we will loop through the clients to see if we have collided with any others
                        for (int j = 0; j < numberOfClients; j++)
                        {
                            Client c2 = clientArray[j];

                            if (!c.Equals(c2) && c.collided(c2))
                            {
                                //client 1 has eaten client 2 and we will increase the weight of client 1 and reset client 2
                                if (c.weight > c2.weight)
                                {
                                    int weightAdded = c2.weight;
                                    c.weight += weightAdded;
									c.score += 10;
                                    c2.weight = 1;
                                    c2.respawn();

                                    for (int k = 0; k < numberOfClients; k++)
                                    {
                                        Client c3 = clientArray[k];
                                        c3.sw.WriteLine("weight\\" + c.clientNumber + "\\" + weightAdded);
										c3.sw.WriteLine ("score\\" + c.clientNumber + "\\" + c.score);
                                        c3.sw.WriteLine("resetPlayer\\" + c2.clientNumber + "\\" + c2.whalePositionX + "\\" + c2.whalePositionY);
                                    }
                                }
                                //client 2 has eaten client 1 and we will increase the weight of client 2 and reset client 1
                                else if (c2.weight > c.weight)
                                {
                                    int weightAdded = c.weight;
                                    c2.weight += weightAdded;
									c2.score += 10;
                                    c.weight = 1;
                                    c.respawn();

                                    for (int k = 0; k < numberOfClients; k++)
                                    {
                                        Client c3 = clientArray[k];
                                        c3.sw.WriteLine("weight\\" + c2.clientNumber + "\\" + weightAdded);
										c3.sw.WriteLine ("score\\" + c2.clientNumber + "\\" + c2.score);
                                        c3.sw.WriteLine("resetPlayer\\" + c.clientNumber + "\\" + c.whalePositionX + "\\" + c.whalePositionY);
                                    }
                                }
                                    
                                //both clients have the same weight and we reset both of their positions and weights
                                else 
                                {
                                    c.weight = 1;
                                    c2.weight = 1;
                                    c.respawn();
                                    c2.respawn();

                                    for (int k = 0; k < numberOfClients; k++)
                                    {
                                        Client c3 = clientArray[k];
                                        c3.sw.WriteLine("resetPlayer\\" + c.clientNumber + "\\" + c.whalePositionX + "\\" + c.whalePositionY);
                                        c3.sw.WriteLine("resetPlayer\\" + c2.clientNumber + "\\" + c2.whalePositionX + "\\" + c2.whalePositionY);
                                    }
                                }
                            }
                        }

                        //if the client collides with any of the borders, reset their weight and position
                        if (c.topWallPosition > TopBorderPosition ||
                            c.bottomWallPosition < BottomBorderPosition ||
                            c.rightWallPosition > RightBorderPosition ||
                            c.leftWallPosition < LeftBorderPosition)
                        {
                            c.respawn();
                            c.weight = 1;

                            for (int j = 0; j < numberOfClients; j++)
                            {
                                Client c2 = clientArray[j];
                                c2.sw.WriteLine("resetPlayer\\" + c.clientNumber + "\\" + c.whalePositionX + "\\" + c.whalePositionY);
                            }
                        }


                        if (c.weight >= winningWeight && playing)
                        {
                            for (int j = 0; j < numberOfClients; j++)
                            {
                                Client c2 = clientArray[j];
                                c2.sw.WriteLine("winningClient\\" + c.clientNumber);
                            }

                            playing = false;
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
            public int weight = 1;
			public int score = 0;
            public float whalePositionX = 0;
            public float whalePositionY = 0;
            public float xVelocity = 0;
            public float yVelocity = 0;
            public float width = 0.3f;
            public float height = 0.4f;

            public float rightWallPosition;
            public float leftWallPosition;
            public float topWallPosition;
            public float bottomWallPosition;

            public bool clientReady = false;
            public bool spawned = false;

            public Client(NetworkStream nws, StreamReader sr, StreamWriter sw, int clientNumber)
            {
                this.nws = nws;
                this.sr = sr;
                this.sw = sw;
                this.clientNumber = clientNumber;

                thread = new Thread(new ThreadStart(this.Service));
                commandQueue = new Queue<string>();

                switch (clientNumber)
                {
                    case(1) :
                        updatePosition(-2.5f, 0);
                        break;
                    case(2) :
                        updatePosition(0, 2.5f);
                        break;
                    case(3) :
                        updatePosition(2.5f, 0);
                        break;
                    case(4) :
                        updatePosition(0, -2.5f);
                        break;
                }
            }


            public void updatePosition(float x, float y)
            {
                whalePositionX = x;
                whalePositionY = y;

                rightWallPosition = whalePositionX + (width * 0.5f);
                leftWallPosition = whalePositionX - (width * 0.5f);
                topWallPosition = whalePositionY + (height * 0.5f);
                bottomWallPosition = whalePositionY - (height * 0.5f);
            }

            public bool collided(Client client)
            {

                return !((client.bottomWallPosition > topWallPosition) ||
                            (client.topWallPosition < bottomWallPosition) ||
                            (client.leftWallPosition > rightWallPosition) ||
                            (client.rightWallPosition < leftWallPosition));
            }

            public void respawn()
            {

                whalePositionX = (float)(rng.NextDouble()) * 4.0f;
                whalePositionY = (float)(rng.NextDouble()) * 4.0f;

                if (rng.NextDouble() < 0.5)
                    whalePositionX *= -1;

                if (rng.NextDouble() < 0.5)
                    whalePositionY *= -1;

                rightWallPosition = whalePositionX + (width * 0.5f);
                leftWallPosition = whalePositionX - (width * 0.5f);
                topWallPosition = whalePositionY + (height * 0.5f);
                bottomWallPosition = whalePositionY - (height * 0.5f);
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

        public class Pellet
        {
            public float weight = 1;
            public float width = 0.25f;
            public float height = 0.25f;
            public float positionX;
            public float positionY;
            public int pelletID;

            public float rightWallPosition;
            public float leftWallPosition;
            public float topWallPosition;
            public float bottomWallPosition;

            protected Rectangle rect;

            public Pellet(int pelletID)
            {
                this.pelletID = pelletID;
                respawn();

            }

            public void respawn()
            {

                positionX = (float)(rng.NextDouble()) * 4.0f;
                positionY = (float)(rng.NextDouble()) * 4.0f;

                if (rng.NextDouble() < 0.5)
                    positionX *= -1;

                if (rng.NextDouble() < 0.5)
                    positionY *= -1;

                rightWallPosition = positionX + (width * 0.5f);
                leftWallPosition = positionX - (width * 0.5f);
                topWallPosition = positionY + (height * 0.5f);
                bottomWallPosition = positionY - (height * 0.5f);
            }

            public bool collided(Client client)
            {

                return !((client.bottomWallPosition > topWallPosition) ||
                            (client.topWallPosition < bottomWallPosition) ||
                            (client.leftWallPosition > rightWallPosition) ||
                            (client.rightWallPosition < leftWallPosition));
            }
        }
    }
}
