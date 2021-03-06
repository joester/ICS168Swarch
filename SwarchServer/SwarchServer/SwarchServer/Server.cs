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
    public class Server
    {



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
        protected static LinkedList<Client> clientArray;
        protected static Pellet[] pelletArray;
        protected static List<String> loginNames;
        protected static LinkedList<int> availableClientNumbers;

        protected static Stopwatch uniClock;
        protected static Stopwatch commandClock;

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

            loginNames = new List<string>();
            availableClientNumbers = new LinkedList<int>();

            for (int i = 1; i <= 4; i++)
                availableClientNumbers.AddLast(i);

            TopBorderPosition = RightBorderPosition = 5.0f;
            BottomBorderPosition = LeftBorderPosition = -5.0f;

            rng = new Random();
            listener = new TcpListener(4188);
            socArray = new Socket[maxPlayers];
            clientArray = new LinkedList<Client>();
            pelletArray = new Pellet[numberOfPellets];

            listenerThead = new Thread(new ThreadStart(this.Listen));

            uniClock = new Stopwatch();
            commandClock = new Stopwatch();

            loop = new ServerLoop(this);
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

                        int clientNumber = availableClientNumbers.First.Value;
                        availableClientNumbers.RemoveFirst();

                        Client client = new Client(this, nws, sr, sw, clientNumber);
                        client.thread.Start();

                        clientArray.AddLast(client);

                        numberOfClients++;


                        //loop.playerConnected();

                        Console.WriteLine("Client " + client.clientNumber + " has connected");
                        Console.WriteLine("There are now " + numberOfClients + " clients");
                        client.sw.WriteLine("client\\" + client.clientNumber);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

        }


        //if a client is disconnected in anyway
        //we must add that client number to the list of available numbers and remove the pointer to this client
        //and inform all other clients that this client has disconnected
        public void RemoveClient(Client client)
        {
            client.thread.Abort();
            availableClientNumbers.AddLast(client.clientNumber);
            clientArray.Remove(client);
            numberOfClients--;
            dm.sendPacket("remove", client.clientName, client.score);


            Console.WriteLine("Name about to be removed {0}", client.clientName);
            loginNames.Remove(client.clientName);
            dm.deleteFromHighScores(client.clientName);
            dm.sendPacket("remove", client.clientName, client.score);

            foreach (Client c in clientArray)
            {
                c.sw.WriteLine("disconnected\\" + client.clientNumber);
            }

            Console.WriteLine("There are now " + numberOfClients + " clients");

            if (clientArray.Count < 1)
            {
                playing = false;
            }
        }

        public class ServerLoop
        {
            private Server serverRef;

            public Thread loopThread;
            public Thread collisionThread;

            public ServerLoop(Server serverRef)
            {
                this.serverRef = serverRef;

                loopThread = new Thread(new ThreadStart(this.loop));
                collisionThread = new Thread(new ThreadStart(this.CheckCollisions));


                uniClock.Start();
            }

            //happens when a player connects, need to update the client objects



            //recursively check if all clients are ready
            public bool ClientsReady(LinkedList<Client> clients, LinkedListNode<Client> currentClient)
            {


                if (currentClient.Next == null)
                    return currentClient.Value.clientReady;
                else
                    return currentClient.Value.clientReady && ClientsReady(clientArray, currentClient.Next);
            }


            //if the game has already started we do not need to check if the clients are ready
            //simply tell the client that has just connected the game info
            public void loadGameState(Client client)
            {
                //tell all the clients the follow:
                //who is in the game
                //the correct positions and velocities of the clients in the game...
                foreach (Client c in clientArray)
                {

                    client.sw.WriteLine("connected\\" + c.clientNumber);
                    client.sw.WriteLine("position\\" + c.clientNumber + "\\" + c.whalePositionX + "\\" + c.whalePositionY);
                    client.sw.WriteLine("veloctiy\\" + c.clientNumber + "\\" + c.xVelocity + "\\" + c.yVelocity);
                    client.sw.WriteLine("score\\" + c.clientNumber + "\\" + c.score);

                    //...the correct weights of each player...
                    if (c.clientNumber != client.clientNumber)
                    {
                        c.sw.WriteLine("connected\\" + client.clientNumber);

                        //...if that client's weight is 1 then we can leave it at the default value
                        //otherwise add to the default value the current weight - 1 (1 is the default value)...
                        if (c.weight > 1)
                            client.sw.WriteLine("weight\\" + c.clientNumber + "\\" + (c.weight - 1));
                    }
                }

                //...positions of the all the pellets
                for (int i = 0; i < numberOfPellets; i++)
                {
                    Pellet pellet = pelletArray[i];
                    client.sw.WriteLine("spawnPellet\\" + pellet.positionX + "\\" + pellet.positionY + "\\" + pellet.pelletID);
                }

                //let the newly connected client start playing the game now that they have the correct game state
                client.sw.WriteLine("start");
            }

            //server loop that checks messages from clients
            public void loop()
            {

                //always run this loop
                while (true)
                {
                    try
                    {
                        //if all connected clients are read, we can start the game
                        if (numberOfClients > 1 && ClientsReady(clientArray, clientArray.First))
                        {
                            //create all the pellets on the server
                            for (int i = 0; i < numberOfPellets; i++)
                            {
                                pelletArray[i] = new Pellet(i);
                            }

                            foreach (Client c in clientArray)
                            {
                                c.clientReady = false;

                                //let all the clients know about each other
                                foreach (Client c2 in clientArray)
                                {

                                    c2.sw.WriteLine("connected\\" + c.clientNumber);

                                }

                                //let all the clients know about the pellets
                                for (int j = 0; j < numberOfPellets; j++)
                                {
                                    Pellet pellet = pelletArray[j];
                                    c.sw.WriteLine("spawnPellet\\" + pellet.positionX + "\\" + pellet.positionY + "\\" + pellet.pelletID);
                                }

                                //now that all the clients have the initial game state
                                //each client can start playing
                                c.sw.WriteLine("start");
                            }

                            //now that the players have started playing
                            //the server should now start checking if any collisions have occured 
                            //collisionThread.Start();
                            commandClock.Start();
                            playing = true;
                        }

                        //check each client in the array
                        //if they have commands waiting in the queue, dequeue and execute that command



                        foreach (Client client in clientArray)
                        {
                            //the server must check if it has recieved any commands from the clients
                            if (client.commandQueue.Count > 0)
                            {
                                //each command is a string that will be delimited by \\
                                string command = client.commandQueue.Dequeue();

                                //each message will be broken into an array of strings
                                string[] tokens = command.Split(new string[] { "\\" }, StringSplitOptions.None);

                                /*
                                 *the commands are as follows
                                 *play - tells the server that the client is ready to start playing the game
                                 *userInfo - tells the server that the client is sending login information
                                 *      -check sthe database and either logins the clients, creates new user info,
                                 *       denies the login attemps
                                 *velocity - broadcasts the sending client's updated velocity
                                 *position - broadcasts the sending client's updated position
                                 *score - broadcasts the correct scores to each client
                                 *lag - adds a delay when sending messages between client and server (for testing purposes)
                                 */

                                if (tokens[0].Equals("play"))
                                {
                                    if (playing)
                                    {
                                        loadGameState(client);
                                    }
                                    else
                                    {
                                        client.clientReady = true;
                                        Console.WriteLine("client " + client.clientNumber + " is ready");
                                    }
                                }


                                //
                                else if (tokens[0].Equals("userInfo"))
                                {

                                    if (!loginNames.Contains(tokens[1]))
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
                                            client.clientName = tokens[1];
                                            dm.insertIntoHighScores(client.clientName, client.score);
                                            dm.printTable();
                                            dm.printHighTable();
                                            dm.sendPacket("add", client.clientName, client.score);
                                        }


                                    }
                                    else
                                    {
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

                                    CheckCollisions();
                                }


                                else if (tokens[0].Equals("lag"))
                                {
                                    DateTime dt = NTPTime.getNTPTime(ref uniClock);
                                    dt.AddMinutes(uniClock.Elapsed.Minutes);
                                    dt.AddSeconds(uniClock.Elapsed.Seconds);
                                    dt.AddMilliseconds(uniClock.ElapsedMilliseconds);

                                    long ticks = dt.Ticks;

                                    //client1.sw.WriteLine("lag\\" + ticks);
                                }
                                else if (tokens[0].Equals("logout"))
                                {
                                    serverRef.RemoveClient(client);
                                    //Console.WriteLine("Name about to be removed {0}", tokens[1]);
                                    //loginNames.Remove(tokens[1]);
                                    //dm.deleteFromHighScores(client.clientName);
                                    //dm.sendPacket("remove", client.clientName, client.score);

                                }
                                else if (tokens[0].Equals("disconnect"))
                                {
                                    dm.sendPacket("remove", client.clientName, client.score);
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

                        Console.WriteLine(e.StackTrace);
                        Console.WriteLine(e.Message);

                    }
                }
            }

            public void CheckCollisions()
            {
                //while (true)
                //{
                //we must check collisions for each client in the game
                foreach (Client c in clientArray)
                {
                    // Client c = clientArray[i];

                    //check if this client has collided with any pellets
                    foreach (Pellet p in pelletArray)
                    {
                        if (p.collided(c))
                        {
                            //if so respawn that pellet and update weights accordingly
                            p.respawn();
                            c.weight += pelletWeight;
                            c.width += 0.1f;
                            c.height += 0.1f;
                            c.updateBorders();
                            c.score += 1;
                            dm.updateHighScores(c.clientName, c.score);
                            dm.sendPacket("update", c.clientName, c.score);
                            dm.printHighTable();



                            foreach (Client c2 in clientArray)
                            {
                                // Client c2 = clientArray[j];
                                c2.sw.WriteLine("respawnPellet\\" + p.positionX + "\\" + p.positionY + "\\" + p.pelletID);

                                //Console.WriteLine("Client " + c2.clientNumber + " was told to respawn pellet " + p.pelletID + " at position " + p.positionX + ", " + p.positionY);

                                c2.sw.WriteLine("weight\\" + c.clientNumber + "\\" + pelletWeight);
                                c2.sw.WriteLine("score\\" + c.clientNumber + "\\" + c.score);
                            }
                        }
                    }

                    //we will loop through the clients to see if we have collided with any others
                    foreach (Client c2 in clientArray)
                    {
                        // c2 = clientArray[j];

                        if (!c.Equals(c2) && c.collided(c2))
                        {
                            //client 1 has eaten client 2 and we will increase the weight of client 1 and reset client 2
                            if (c.weight > c2.weight)
                            {
                                int weightAdded = c2.weight;
                                c.weight += weightAdded;
                                c.width += ((float)weightAdded * 0.05f);
                                c.updateBorders();
                                c.score += 10;
                                dm.updateHighScores(c.clientName, c.score);
                                dm.sendPacket("update", c.clientName, c.score);
                                dm.printHighTable();
                                c2.weight = 1;
                                c2.respawn();

                                foreach (Client c3 in clientArray)
                                {
                                    // Client c3 = clientArray[k];
                                    c3.sw.WriteLine("weight\\" + c.clientNumber + "\\" + weightAdded);
                                    c3.sw.WriteLine("score\\" + c.clientNumber + "\\" + c.score);
                                    c3.sw.WriteLine("resetPlayer\\" + c2.clientNumber + "\\" + c2.whalePositionX + "\\" + c2.whalePositionY);
                                }
                            }

                            //client 2 has eaten client 1 and we will increase the weight of client 2 and reset client 1
                            else if (c2.weight > c.weight)
                            {
                                int weightAdded = c.weight;
                                c2.weight += weightAdded;
                                c2.width += ((float)weightAdded * 0.05f);
                                c2.updateBorders();
                                c2.score += 10;
                                dm.updateHighScores(c2.clientName, c2.score);
                                dm.sendPacket("update", c2.clientName, c2.score);
                                dm.printHighTable();
                                c.weight = 1;
                                c.respawn();

                                foreach (Client c3 in clientArray)
                                {
                                    //Client c3 = clientArray[k];
                                    c3.sw.WriteLine("weight\\" + c2.clientNumber + "\\" + weightAdded);
                                    c3.sw.WriteLine("score\\" + c2.clientNumber + "\\" + c2.score);
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

                                foreach (Client c3 in clientArray)
                                {
                                    //Client c3 = clientArray[k];
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

                        foreach (Client c2 in clientArray)
                        {
                            //Client c2 = clientArray[j];
                            c2.sw.WriteLine("resetPlayer\\" + c.clientNumber + "\\" + c.whalePositionX + "\\" + c.whalePositionY);
                        }
                    }

                    //if a client has reached the winning weight, we will broadcast this to all the clients
                    if (c.weight >= winningWeight && playing)
                    {
                        dm.sendPacket("gameover", c.clientName, c.clientNumber);
                        foreach (Client c2 in clientArray)
                        {
                            // Client c2 = clientArray[j];
                            c2.sw.WriteLine("winningClient\\" + c.clientName);
                        }

                        //playing = false;
                    }
                }
            }
            // }

        }

        //client class that defines all the attributes 
        public class Client
        {
            private Server serverRef;
            public NetworkStream nws;
            public StreamReader sr;
            public StreamWriter sw;
            public Queue<string> commandQueue;
            public Thread thread;

            public int clientNumber;
            public string clientName;
            public int weight = 1;
            public int score = 0;
            public float whalePositionX = 0;
            public float whalePositionY = 0;
            public float xVelocity = 0;
            public float yVelocity = 0;
            public float width = 0.4f;
            public float height = 0.4f;

            public float rightWallPosition;
            public float leftWallPosition;
            public float topWallPosition;
            public float bottomWallPosition;

            public bool clientReady = false;
            public bool spawned = false;

            //each client must keep track of the network stream between itself and the server
            //the stream reader used by the server to read in client messages
            //the stream writer used by the server to send the client messages
            //an assigned client number
            public Client(Server serverRef, NetworkStream nws, StreamReader sr, StreamWriter sw, int clientNumber)
            {
                this.serverRef = serverRef;
                this.nws = nws;
                this.sr = sr;
                this.sw = sw;
                this.clientNumber = clientNumber;

                //each client has a thread that the server uses to alternate reading in messages from
                thread = new Thread(new ThreadStart(this.Service));
                commandQueue = new Queue<string>();

                switch (clientNumber)
                {
                    case (1):
                        updatePosition(-2.5f, 0);
                        break;
                    case (2):
                        updatePosition(0, 2.5f);
                        break;
                    case (3):
                        updatePosition(2.5f, 0);
                        break;
                    case (4):
                        updatePosition(0, -2.5f);
                        break;
                }
            }

            //whenever the position is changed we must also update the positions of the walls of the player
            public void updatePosition(float x, float y)
            {
                whalePositionX = x;
                whalePositionY = y;

                rightWallPosition = whalePositionX + (width * 0.5f);
                leftWallPosition = whalePositionX - (width * 0.5f);
                topWallPosition = whalePositionY + (height * 0.5f);
                bottomWallPosition = whalePositionY - (height * 0.5f);
            }

            //we will collisions based on the position of the walls
            //if any of those conditions are true then it is impossible for a collision to have occured
            public bool collided(Client client)
            {

                return !((client.bottomWallPosition > topWallPosition) ||
                            (client.topWallPosition < bottomWallPosition) ||
                            (client.leftWallPosition > rightWallPosition) ||
                            (client.rightWallPosition < leftWallPosition));
            }

            //respawn the player in a random position on the screen
            public void respawn()
            {

                Console.WriteLine("Client " + this.clientNumber + " was told to respawn");

                whalePositionX = (float)(rng.NextDouble()) * 4.0f;
                whalePositionY = (float)(rng.NextDouble()) * 4.0f;

                if (rng.NextDouble() < 0.5)
                    whalePositionX *= -1;

                if (rng.NextDouble() < 0.5)
                    whalePositionY *= -1;

                width = height = 0.4f;

                updateBorders();

            }

            public void updateBorders()
            {

                rightWallPosition = whalePositionX + (width * 0.5f);
                leftWallPosition = whalePositionX - (width * 0.5f);
                topWallPosition = whalePositionY + (height * 0.5f);
                bottomWallPosition = whalePositionY - (height * 0.5f);
            }

            //checks if the client has sent any messages
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
                    //Console.WriteLine(e.InnerException);
                    //If the steam closes we will catch the exception and remove the client from the game
                    //properly
                    //Server.RemoveClient(this);
                    //Console.WriteLine(e.StackTrace);
                    serverRef.RemoveClient(this);
                    Console.WriteLine(e.Message + " Client " + this.clientNumber + " has disconnected");

                }
            }
        }

        //pellet class that defines the properties of a pellet
        public class Pellet
        {
            public float weight = 1;
            public float width = 0.35f;
            public float height = 0.35f;
            public float positionX;
            public float positionY;
            public int pelletID;

            public float rightWallPosition;
            public float leftWallPosition;
            public float topWallPosition;
            public float bottomWallPosition;

            //each pellet has a unique id to keep track of them
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
