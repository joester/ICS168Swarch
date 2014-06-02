using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System;
using System.Diagnostics;
using System.Threading;

public class Sockets {
	

	private string SERVER_LOCATION = "169.234.19.52";
    const int SERVER_PORT = 4188; //FILL THESE OUT FOR YOUR OWN SERVER

	public TcpClient client;

	public NetworkStream nws;
	public StreamWriter sw;
	public int clientNumber;
	public bool connected;
	
	public DateTime dt;
	
	public Thread t = null; 
		
	protected static bool threadState = false;
	
	public Queue recvBuffer;
	
	public Sockets()
	{		
		connected = false;
		recvBuffer = new Queue();
	}

	//sets the IP to a new IP as entered by the user
	public void setIP(string newIP)
	{
		SERVER_LOCATION = newIP;
	}

	//returns the current IP
	public string getIP()
	{
		return SERVER_LOCATION;
	}

	//connects the client to the server
	public bool Connect ()
	{
		//ESTABLISH CONNECTION THEN MAKE THREAD TO READ BYTES FROM STREAM
		try
		{
            client = new TcpClient(SERVER_LOCATION, SERVER_PORT);
			nws = client.GetStream();
			ThreadSock tsock = new ThreadSock(nws, this);
			t = new Thread (new ThreadStart(tsock.Service));
			t.IsBackground = true;
			t.Start();
			threadState = true;
			sw = new StreamWriter(nws);
			sw.AutoFlush = true;
		}

		catch ( Exception ex )
		{
			Console.WriteLine ( ex.Message + " : OnConnect");			
		}
		
		if ( client == null ) return false;
		return client.Connected;
	}

	//disconnects the client from the server gracefully
	public bool Disconnect ()
	{			
		try
		{
			SendTCPPacket("disconnect\\" + GameObject.Find("GameProcess").GetComponent<GameProcess>().clientNumber);
            client.GetStream().Close();
            client.Close();			
		}
		catch ( Exception ex )
		{
			Console.WriteLine ( ex.Message + " : OnDisconnect" );
			return false;			
		}

		return true;
	}

	//the workhorse of the project, sends a string to the server
	public void SendTCPPacket ( string toSend )
	{
		try
		{
			sw.WriteLine(toSend);
		}
		catch ( Exception ex )
		{
			Console.WriteLine ( ex.Message + ": OnTCPPacket" );
		}	
	}	
		
	public void endThread(){
		threadState = false;
	}
	
	public void testThread()
	{		
		try
		{
			if ( t!= null && !threadState  )
			{
				Console.WriteLine ( "thread aborted");
				t.Abort();
				threadState = !threadState;	
			}
		}
		catch ( Exception ex )
		{
			Console.WriteLine ( ex.Message + " : testThread ");
		}
	}
	
}
