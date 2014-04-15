using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;

public class GameProcess : MonoBehaviour {

	//PUBLIC MEMBERS 
	//public bool play;
	public int clientNumber;
	public string playerName;
	GameObject Player1;
	GameObject Player2;
	public int winningClientNumber;
	public int winningWeight;
	int numStartingPellets;

	//PRIVATE MEMBERS
	private Sockets socks;
	private string stringBuffer;
	private string tempBuffer;

	public DateTime dT;
	public Stopwatch uniClock;

	// Use this for initialization
	void Start () {
		uniClock = new Stopwatch();
		winningWeight = 10;

		//play = false;
		socks = new Sockets();

		Player1 = GameObject.Find("Player1");
		Player2 = GameObject.Find ("Player2");

		winningClientNumber = -1;
		numStartingPellets = 5;

		for (int i = 0; i < numStartingPellets; i++)
		{
			Instantiate((GameObject)Resources.Load("Pellet"), new Vector3(UnityEngine.Random.Range(-4f,4f), UnityEngine.Random.Range(-4f,4f), 0), transform.rotation);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (socks.recvBuffer.Count > 0)
		{
			//Dequeue the single-line string sent from the server
			stringBuffer = (string)socks.recvBuffer.Dequeue();

			//separate the string by its delimiter '\\' to parse the line's content
			string[] tokens = stringBuffer.Split(new string[] {"\\"}, StringSplitOptions.None);

			//Determine the content of the string sent from the sever

				// hit\\posX\\posY\\VelX\\VelY\\time
//			else if (tokens[0].Equals("hit"))
//			{
//				float xPos = Convert.ToSingle(tokens[1]);
//				float yPos = Convert.ToSingle(tokens[2]);
//				float xVel = Convert.ToSingle(tokens[3]);
//				float yVel = Convert.ToSingle(tokens[4]);
//
//				ball.GetComponent<BallScript>().xVelocity = xVel;
//				ball.GetComponent<BallScript>().yVelocity = yVel;
//
//				dT = NTPTime.getNTPTime(ref uniClock);
//				dT.AddMinutes(uniClock.Elapsed.Minutes);
//				dT.AddSeconds(uniClock.Elapsed.Seconds);
//				dT.AddMilliseconds(uniClock.Elapsed.Milliseconds);
//
//				long clientTime = dT.Ticks;
//				long serverTime = Convert.ToInt64(tokens[5]);
//
//				long difference = clientTime - serverTime;
//
//				UnityEngine.Debug.Log ("Latency: " + difference);
//
//				TimeSpan latency = new TimeSpan(difference);
//
//				xPos += (latency.Milliseconds / 20) * xVel;
//				yPos += (latency.Milliseconds / 20) * yVel;
//
//
//				ball.transform.position = new Vector3(xPos, yPos, 0);
//			}


			// "start\\xVelocity\\yVelocity"


//			else if(tokens[0].Equals("lag"))
//			{
//				dT = NTPTime.getNTPTime(ref uniClock);
//				dT.AddMinutes(uniClock.Elapsed.Minutes);
//				dT.AddSeconds(uniClock.Elapsed.Seconds);
//				dT.AddMilliseconds(uniClock.Elapsed.Milliseconds);
//
//				long clientTime = dT.Ticks;
//				long serverTime = Convert.ToInt64(tokens[1]);
//				
//				long difference = clientTime - serverTime;
//
//				TimeSpan latency = new TimeSpan(difference);
//
//				GameObject.Find("LatencyText").guiText.text = latency.Milliseconds.ToString() + " ms";
			//}

//			else if (tokens[0].Equals("gameover"))
//			{
//				UnityEngine.Debug.Log ("received");
//				GameObject.Find ("GUI").GetComponent<Gui>().resetGame = true;
//				winningClientNumber = Int32.Parse(tokens[1]);
//			}
//		}
//	}
		}
	}

	public Sockets returnSocket ()
	{
		return socks;
	}
}
