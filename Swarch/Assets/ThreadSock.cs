//AFSHIN MAHINI 2013 - 2014

using UnityEngine;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;

public class ThreadSock
{
	private NetworkStream nws;
	private Sockets socks;

	private StreamReader reader;
	
	//********* COMPLETE THE FOLLOWING CODE
	public ThreadSock (NetworkStream nwsIn, Sockets inSocket)
	{
        nws = nwsIn;
        socks = inSocket;
		reader = new StreamReader(nws);
	}
	//********* COMPLETE THE FOLLOWING CODE
	//********* READ THE STREAM, ADD TO QUEUE, BE THREAD SAFE
	public void Service ()
	{	
		try
		{
			while (true)
			{
				string data = reader.ReadLine();
				lock(socks.recvBuffer)
				{
					socks.recvBuffer.Enqueue(data);
				}
			}
		}

		catch ( Exception ex )
		{
			Console.WriteLine ( ex.Message + " : Thread loop" );
			
		}
		
	}
	
}
