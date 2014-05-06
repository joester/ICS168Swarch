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
	public bool play;

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

		play = false;
	
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

			if(tokens[0].Equals("client"))
			{
				// "client\\clientNumber"
				//UnityEngine.Debug.Log(Int32.Parse(tokens[1]));
				clientNumber = Int32.Parse(tokens[1]);
			}

			else if (tokens[0].Equals("accountCreated"))
			{
				GameObject.Find("Login_GUI").GetComponent<LoginScreenGUI>().guiText.text = "Account Created.";
			}

			else if (tokens[0].Equals("loginFail"))
			{
				GameObject.Find("Login_GUI").GetComponent<LoginScreenGUI>().loginFail();
			}

			else if (tokens[0].Equals("loginSucceed"))
			{
				GameObject.Find("Login_GUI").GetComponent<LoginScreenGUI>().loginSucceed();
				playerName = tokens[1];
			}

			else if (tokens[0].Equals("connected"))
			{
				Quaternion facingUp = new Quaternion(0,0,0,1);
				int clientThatJustConnected = Int32.Parse (tokens[1]);
				switch (clientThatJustConnected)
				{
				case 1:
					GameObject.Instantiate(Resources.Load ("Player1"), new Vector3(-2.5f,0f,0f), facingUp);
					break;
				case 2:
					GameObject.Instantiate(Resources.Load ("Player2"), new Vector3(0f,2.5f,0f), facingUp);
					break;
				case 3:
					GameObject.Instantiate(Resources.Load ("Player3"), new Vector3(2.5f,0f,0f), facingUp);
					break;
				case 4:
					GameObject.Instantiate(Resources.Load ("Player4"), new Vector3(0f,-2.5f,0f), facingUp);
					break;					
				}
			}
			
			else if (tokens[0].Equals("start"))
			{			
				play = true;
				GameObject.Find("GameGUI").GetComponent<GameGUIScript>().guiText.text = "";
				GameObject.Find("GameGUI").GetComponent<GameGUIScript>().show = false;
				//for (int i = 0; i < numStartingPellets; i++)
				//{
				//	Instantiate((GameObject)Resources.Load("Pellet"), new Vector3(UnityEngine.Random.Range(-4f,4f), UnityEngine.Random.Range(-4f,4f), 0), transform.rotation);
				//}
			}

			else if (tokens[0].Equals("position"))
			{
				if(tokens[1].Equals("1"))
				{
					if (clientNumber != 1)
						GameObject.Find ("Player1").GetComponent<Player1Script>().transform.position = 
							new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0);
				}

				if(tokens[1].Equals("2"))
				{
					if (clientNumber != 2)
						GameObject.Find ("Player2").GetComponent<Player2Script>().transform.position = 
							new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0);
				}

				if(tokens[1].Equals("3"))
				{
					if (clientNumber != 3)
						GameObject.Find ("Player3").GetComponent<Player3Script>().transform.position = 
							new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0);
				}
				
				if(tokens[1].Equals("4"))
				{
					if (clientNumber != 4)
						GameObject.Find ("Player4").GetComponent<Player4Script>().transform.position = 
							new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0);
				}
			}

			else if (tokens[0].Equals("velocity"))
			{
				if(tokens[1].Equals("1"))
				{
					if (clientNumber != 1)
					{
						GameObject.Find ("Player1").GetComponent<Player1Script>().xVelocity = Convert.ToSingle (tokens[2]);
						GameObject.Find ("Player1").GetComponent<Player1Script>().yVelocity = Convert.ToSingle (tokens[3]);
					}
				}
				
				if(tokens[1].Equals("2"))
				{
					if (clientNumber != 2)
					{
						GameObject.Find ("Player2").GetComponent<Player2Script>().xVelocity = Convert.ToSingle (tokens[2]);
						GameObject.Find ("Player2").GetComponent<Player2Script>().yVelocity = Convert.ToSingle (tokens[3]);
					}
				}

				if(tokens[1].Equals("3"))
				{
					if (clientNumber != 3)
					{
						GameObject.Find ("Player3").GetComponent<Player3Script>().xVelocity = Convert.ToSingle (tokens[2]);
						GameObject.Find ("Player3").GetComponent<Player3Script>().yVelocity = Convert.ToSingle (tokens[3]);
					}
				}
				
				if(tokens[1].Equals("4"))
				{
					if (clientNumber != 4)
					{
						GameObject.Find ("Player4").GetComponent<Player4Script>().xVelocity = Convert.ToSingle (tokens[2]);
						GameObject.Find ("Player4").GetComponent<Player4Script>().yVelocity = Convert.ToSingle (tokens[3]);
					}
				}
			}

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


			else if(tokens[0].Equals("lag"))
			{
				dT = NTPTime.getNTPTime(ref uniClock);
				dT.AddMinutes(uniClock.Elapsed.Minutes);
				dT.AddSeconds(uniClock.Elapsed.Seconds);
				dT.AddMilliseconds(uniClock.Elapsed.Milliseconds);

				long clientTime = dT.Ticks;
				long serverTime = Convert.ToInt64(tokens[1]);
				
				long difference = clientTime - serverTime;

				TimeSpan latency = new TimeSpan(difference);

				GameObject.Find("LatencyText").guiText.text = latency.Milliseconds.ToString() + " ms";
			}

//			else
//			{
//				string packet = "Unrecognized packet: ";
//				foreach (string i in tokens)
//				{
//					packet += "\\" + i;
//				}
//				UnityEngine.Debug.Log(packet);
//			}

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
