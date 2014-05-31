using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Data.SQLite;
using System.Net;
using System.Text;

namespace SwarchServer
{
	public class Scoreboard
	{
		IPEndPoint RemoteEndPoint;
		
		Socket s;


		public Scoreboard ()
		{
			RemoteEndPoint = new IPEndPoint (IPAddress.Any, 9000);
			s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}
		public void sendMessage(String name, int score)
		{

			Byte[] data1 = Encoding.ASCII.GetBytes(name + score);
			Byte[] data2 = Encoding.ASCII.GetBytes(score.ToString());
			s.SendTo(data1, data1.Length, SocketFlags.None, RemoteEndPoint);
			s.SendTo(data2, data2.Length, SocketFlags.None, RemoteEndPoint);


		}
	}
}

