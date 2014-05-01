using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace Project1
{
    public class Server
    {
        protected static int maxPlayers;
        protected static int numberOfClients;
        protected static int playerOneScore;
        protected static int playerTwoScore;
        protected static int winningScore;

        protected static TcpListener listener;
        protected static Socket[] socArray;
        protected static Client[] clientArray;
        protected static Stopwatch uniClock;
        protected static Client client1;
        protected static Client client2;

        protected static float ballVelocityX;
        protected static float ballVelocityY;
        protected static float paddleOnePositionY;
        protected static float paddleTwoPositionY;

        protected static bool playing = false;

        //we'll use this code as a basis for our swarch server
        public Server()
        {
            
            uniClock = new Stopwatch();
 
            maxPlayers = 4;
            numberOfClients = 0;
            listener = new TcpListener(4185);
            socArray = new Socket[maxPlayers];
            clientArray = new Client[maxPlayers];

        }

        public static void resetBallVelocity()
        {
            Random r = new Random();

            ballVelocityX = Convert.ToSingle(r.NextDouble());
            ballVelocityY = Convert.ToSingle(r.NextDouble());

            while (ballVelocityX < .4 || ballVelocityX > .6
                && ballVelocityY < .4 || ballVelocityY > .6)
            {
                ballVelocityX = Convert.ToSingle(r.NextDouble());
                ballVelocityY = Convert.ToSingle(r.NextDouble());
            }

            if (r.NextDouble() < .5)
                ballVelocityX *= -1;

            if (r.NextDouble() < .5)
                ballVelocityY *= -1;


            ballVelocityX *= .04f;
            ballVelocityY *= .04f;
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
                    sw.WriteLine("client\\" +  numberOfClients);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            ServerLoop serverLoop = new ServerLoop(clientArray);
            serverLoop.loop();
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
                BallControl bc;

                while (true)
                {
                    if (client1Ready && client2Ready)
                    {
                        resetBallVelocity();
                        bc = new BallControl();
                        
                        client1.sw.WriteLine("start\\" + ballVelocityX + "\\" + +ballVelocityY);
                        client2.sw.WriteLine("start\\" + ballVelocityX + "\\" + +ballVelocityY);

                        client1Ready = false;
                        client2Ready = false;

                        playing = true;
                        bc.ballTimer.Restart();
                        
                    }

                    if (client1.commandQueue.Count > 0)
                    {
                        string command = client1.commandQueue.Dequeue();

                        string[] tokens = command.Split(new string[] { "\\" }, StringSplitOptions.None);

                        if (tokens[0].Equals("play"))
                        {
                            client1Ready = true;
                        }

                        else if (tokens[0].Equals("paddle") && tokens[1].Equals("1"))
                        {
                            Thread.Sleep((int)Convert.ToSingle(tokens[3]) * 1000);

                            client2.sw.WriteLine("paddle\\" + 1 + "\\" + tokens[2]);
                            paddleOnePositionY = Convert.ToSingle(tokens[2]);
                        }

                        else if (tokens[0].Equals("score"))
                        {
                            client1.sw.WriteLine("score\\"+ tokens[1]);
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

                        else if (tokens[0].Equals("paddle") && tokens[1].Equals("2"))
                        {

                            Thread.Sleep((int)Convert.ToSingle(tokens[3]) * 1000);

                            client1.sw.WriteLine("paddle\\" + 2 + "\\" + tokens[2]);
                            paddleTwoPositionY = Convert.ToSingle(tokens[2]);
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

        //client class
        //has network stream, stream reader and stream writer
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

                        lock(commandQueue)
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

        public class BallControl
        {
            float ballPositionX = 0;
            float ballPositionY = 0;
            public Stopwatch ballTimer;
            public Thread thread;
            public long offset;
            public bool addOffset;

            public BallControl()
            {
                ballTimer = new Stopwatch();
                ballTimer.Start();

                thread = new Thread(new ThreadStart(this.BallLoop));
                thread.Start();                

                offset = 0;
      
            }

            public void BallLoop()
            {
                long elapsedTime = 0;                

                while(true)
                {
                    if (playing)
                    {

                    elapsedTime = ballTimer.ElapsedMilliseconds;

                    if (elapsedTime + offset >= 20)
                    {
                        if (elapsedTime + offset > 20)
                        {
                            for (int i = 0; i < elapsedTime + offset / 20; i++)
                            {
                                elapsedTime = ballTimer.ElapsedMilliseconds;
                                elapsedTime -= i * 20;

                                ballPositionX += ballVelocityX;
                                ballPositionY += ballVelocityY;

                                if (ballPositionX != 0)
                                    Console.WriteLine("PositionX: " + ballPositionX + " ; PositionY: " + ballPositionY);

                                if (ballPositionY > 2.6 || ballPositionY < -2.6)
                                    ballVelocityY *= -1;

                                if (ballPositionX <= -3.5f)
                                {
                                    DateTime dt = NTPTime.getNTPTime(ref uniClock);
                                    dt.AddMinutes(uniClock.Elapsed.Minutes);
                                    dt.AddSeconds(uniClock.Elapsed.Seconds);
                                    dt.AddMilliseconds(uniClock.ElapsedMilliseconds);

                                    long ticks = dt.Ticks;

                                    ballVelocityX *= -1;

                                    client1.sw.WriteLine("hit\\" + ballPositionX + "\\" + ballPositionY + "\\" + ballVelocityX + "\\" + ballVelocityY + "\\" + ticks);
                                    client2.sw.WriteLine("hit\\" + ballPositionX + "\\" + ballPositionY + "\\" + ballVelocityX + "\\" + ballVelocityY + "\\" + ticks);

                                    Thread.Sleep(500);

                                    if (!(ballPositionY > paddleOnePositionY - 0.8f && ballPositionY < paddleOnePositionY + 0.8f))
                                    {

                                        client1.sw.WriteLine("score\\" + 2);
                                        client2.sw.WriteLine("score\\" + 2);
                                        ballPositionX = 0;
                                        ballPositionY = 0;

                                        ballVelocityX = 0.0f;
                                        ballVelocityY = 0.0f;

                                        client1.sw.WriteLine("reset");
                                        client2.sw.WriteLine("reset");

                                        thread.Abort();
                                        playing = false;


                                    }

                                }
                                else if (ballPositionX >= 3.5f)
                                {
                                    DateTime dt = NTPTime.getNTPTime(ref uniClock);
                                    dt.AddMinutes(uniClock.Elapsed.Minutes);
                                    dt.AddSeconds(uniClock.Elapsed.Seconds);
                                    dt.AddMilliseconds(uniClock.ElapsedMilliseconds);

                                    long ticks = dt.Ticks;

                                    ballVelocityX *= -1;

                                    client1.sw.WriteLine("hit\\" + ballPositionX + "\\" + ballPositionY + "\\" + ballVelocityX + "\\" + ballVelocityY + "\\" + ticks);
                                    client2.sw.WriteLine("hit\\" + ballPositionX + "\\" + ballPositionY + "\\" + ballVelocityX + "\\" + ballVelocityY + "\\" + ticks);

                                    Thread.Sleep(500);

                                    if (ballPositionY > paddleTwoPositionY - 0.8f && ballPositionY < paddleTwoPositionY + 0.8f)
                                    {

                                    }
                                    else
                                    {

                                        client1.sw.WriteLine("score\\" + 1);
                                        client2.sw.WriteLine("score\\" + 1);
                                        ballPositionX = 0;
                                        ballPositionY = 0;

                                        ballVelocityX = 0.0f;
                                        ballVelocityY = 0.0f;


                                        client1.sw.WriteLine("reset");
                                        client2.sw.WriteLine("reset");

                                        thread.Abort();
                                        playing = false;
                                    }
                                }
                            }
                        }

                        else // elapsedTime + offset == 20
                        {
                            ballPositionX += ballVelocityX;
                            ballPositionY += ballVelocityY;

                            if (ballPositionX != 0)
                                Console.WriteLine("PositionX: " + ballPositionX + " ; PositionY: " + ballPositionY);

                            if (ballPositionY > 2.6 || ballPositionY < -2.6)
                                ballVelocityY *= -1;

                            if (ballPositionX <= -3.5f)
                            {
                                DateTime dt = NTPTime.getNTPTime(ref uniClock);
                                dt.AddMinutes(uniClock.Elapsed.Minutes);
                                dt.AddSeconds(uniClock.Elapsed.Seconds);
                                dt.AddMilliseconds(uniClock.ElapsedMilliseconds);

                                long ticks = dt.Ticks;

                                ballVelocityX *= -1;

                                client1.sw.WriteLine("hit\\" + ballPositionX + "\\" + ballPositionY + "\\" + ballVelocityX + "\\" + ballVelocityY + "\\" + ticks);
                                client2.sw.WriteLine("hit\\" + ballPositionX + "\\" + ballPositionY + "\\" + ballVelocityX + "\\" + ballVelocityY + "\\" + ticks);

                                Thread.Sleep(500);

                                if (ballPositionY > paddleOnePositionY - 0.8f && ballPositionY < paddleOnePositionY + 0.8f)
                                {

                                }
                                else
                                {

                                    client1.sw.WriteLine("score\\" + 2);
                                    client2.sw.WriteLine("score\\" + 2);
                                    ballPositionX = 0;
                                    ballPositionY = 0;

                                    ballVelocityX = 0.0f;
                                    ballVelocityY = 0.0f;

                                    client1.sw.WriteLine("reset");
                                    client2.sw.WriteLine("reset");


                                    thread.Abort();
                                    playing = false;
                                }

                            }
                            else if (ballPositionX >= 3.5f)
                            {
                                DateTime dt = NTPTime.getNTPTime(ref uniClock);
                                dt.AddMinutes(uniClock.Elapsed.Minutes);
                                dt.AddSeconds(uniClock.Elapsed.Seconds);
                                dt.AddMilliseconds(uniClock.ElapsedMilliseconds);

                                long ticks = dt.Ticks;

                                ballVelocityX *= -1;

                                client1.sw.WriteLine("hit\\" + ballPositionX + "\\" + ballPositionY + "\\" + ballVelocityX + "\\" + ballVelocityY + "\\" + ticks);
                                client2.sw.WriteLine("hit\\" + ballPositionX + "\\" + ballPositionY + "\\" + ballVelocityX + "\\" + ballVelocityY + "\\" + ticks);

                                Thread.Sleep(500);

                                if (ballPositionY > paddleTwoPositionY - 0.8f && ballPositionY < paddleTwoPositionY + 0.8f)
                                {

                                }
                                else
                                {
                                    client1.sw.WriteLine("score\\" + 1);
                                    client2.sw.WriteLine("score\\" + 1);
                                    ballPositionX = 0;
                                    ballPositionY = 0;

                                    ballVelocityX = 0.0f;
                                    ballVelocityY = 0.0f;

                                    client1.sw.WriteLine("reset");
                                    client2.sw.WriteLine("reset");

                                    thread.Abort();
                                    playing = false;
                                }
                            }

                            //Console.WriteLine("PositionX: " + ballPositionX + " ; PositionY: " + ballPositionY);                 
                        }

                        offset = elapsedTime %= 20;
                        ballTimer.Restart();
                    }
                    }
                }
            }        
        }
    }
}
