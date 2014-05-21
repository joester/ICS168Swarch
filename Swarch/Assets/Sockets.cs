		//AFSHIN MAHINI 2013 - 2014

using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System;
using System.Diagnostics;
using System.Threading;


public class Sockets {


    //const string SERVER_LOCATION = "128.195.11.128"; 
	//const string SERVER_LOCATION = "169.234.22.64"; 
<<<<<<< HEAD
	const string SERVER_LOCATION = "169.234.86.178";
=======
	const string SERVER_LOCATION = "169.234.46.45";
>>>>>>> FETCH_HEAD
	//const string SERVER_LOCATION = "169.234.46.36";
    const int SERVER_PORT = 4185; //FILL THESE OUT FOR YOUR OWN SERVER
	
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
		
		//uniClock = new Stopwatch();
		//dt = NTPTime.getNTPTime ( dt, ref uniClock );
		
	}
	
	public bool Connect ()
	{
		//********* COMPLETE THE FOLLOWING CODE
		//********* ESTABLISH CONNECTION THEN MAKE THREAD TO READ BYTES FROM STREAM
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
	
	public bool Disconnect ()
	{
		//********* COMPLETE THE FOLLOWING CODE		
		try
		{
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
	
	public void SendTCPPacket ( string toSend )
	{
		//********* COMPLETE THE FOLLOWING CODE
		try
		{
			sw.WriteLine(toSend);
		}
		catch ( Exception ex )
		{
			Console.WriteLine ( ex.Message + ": OnTCPPacket" );
		}	
	}
	
	public void measureLatency () //UN-NECESSARY
	{
	}
	
	public int returnLatency(){//UN-NECESSARY
		//return latency;
		return 0;
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
