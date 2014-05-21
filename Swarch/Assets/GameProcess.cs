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


			///////////////////// DEBUG - WRITE ALL COMMANDS RECEIVED /////////////////////////
			String s = "";
			for (int j = 0; j < tokens.Length; j++)
				s+= tokens[j] + " ";
			UnityEngine.Debug.Log(s);
			///////////////////////////////////////////////////////////////////////////////////


			//Determine the content of the string sent from the server

			//client\\clientNumber
			if(tokens[0].Equals("client"))
			{
				clientNumber = Int32.Parse(tokens[1]);
			}

			//loginFail
			else if (tokens[0].Equals("loginFail"))
			{
				GameObject.Find("Login_GUI").GetComponent<LoginScreenGUI>().loginFail();
			}

			//loginSucceed\\correctUsername
			else if (tokens[0].Equals("loginSucceed"))
			{
				GameObject.Find("Login_GUI").GetComponent<LoginScreenGUI>().loginSucceed();
				playerName = tokens[1];
			}

			//connected\\clientNumberThatConnected
			else if (tokens[0].Equals("connected"))
			{
				//this quaternion is used to make sure the player's avatar is correctly oriented
				Quaternion facingUp = new Quaternion(0,0,0,1);

				int clientThatJustConnected = Int32.Parse (tokens[1]);
				//UnityEngine.Debug.Log (clientNumber + " was told to spawn " + clientThatJustConnected);

				//spawns a player avatar corresponding to the information from the packet (see above)
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

			//start allows the players to move their avatars and removes gui elements
			else if (tokens[0].Equals("start"))
			{			
				play = true;
				GameObject.Find("GameGUI").GetComponent<GameGUIScript>().guiText.text = "";
				GameObject.Find("GameGUI").GetComponent<GameGUIScript>().showLogout = false;
			}

			//used to update the position of an avatar with the below syntax
			//position\\clientNumber\\xPosition\\yPosition
			else if (tokens[0].Equals("position"))
			{
				//move player 1's avatar...
				if(tokens[1].Equals("1"))
				{
					//...if this client is not client 1 in which case it already has more recent position info
					if (clientNumber != 1)
						GameObject.FindGameObjectWithTag ("Player1").transform.position = 
							new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0);
				}

				//move player 2's avatar...
				else if(tokens[1].Equals("2"))
				{
					//...if this client is not client 2 in which case it already has more recent position info
					if (clientNumber != 2)
						GameObject.FindGameObjectWithTag ("Player2").transform.position = 
							new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0);
				}

				//move player 3's avatar...
				else if(tokens[1].Equals("3"))
				{
					//...if this client is not client 3 in which case it already has more recent position info
					if (clientNumber != 3)
						GameObject.FindGameObjectWithTag ("Player3").transform.position = 
							new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0);
				}

				//move player 4's avatar...
				else if(tokens[1].Equals("4"))
				{
					//...if this client is not client 4 in which case it already has more recent position info
					if (clientNumber != 4)
						GameObject.FindGameObjectWithTag ("Player4").transform.position = 
							new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0);
				}
			}

			//change an avatar's velocity according to the following syntax
			//velocity\\clientNumber\\xVelocity\\yVelocity
			else if (tokens[0].Equals("velocity"))
			{
				//set player 1's velocity...
				if(tokens[1].Equals("1"))
				{
					//...if this client is not client 1 in which case it already has more recent velocity info
					if (clientNumber != 1)
					{
						GameObject.FindGameObjectWithTag("Player1").GetComponent<Player1Script>().xVelocity = Convert.ToSingle (tokens[2]);
						GameObject.FindGameObjectWithTag("Player1").GetComponent<Player1Script>().yVelocity = Convert.ToSingle (tokens[3]);
					}
				}

				//set player 2's velocity...
				else if(tokens[1].Equals("2"))
				{
					//...if this client is not client 2 in which case it already has more recent velocity info
					if (clientNumber != 2)
					{
						GameObject.FindGameObjectWithTag ("Player2").GetComponent<Player2Script>().xVelocity = Convert.ToSingle (tokens[2]);
						GameObject.FindGameObjectWithTag ("Player2").GetComponent<Player2Script>().yVelocity = Convert.ToSingle (tokens[3]);
					}
				}

				//set player 3's velocity...
				else if(tokens[1].Equals("3"))
				{
					//...if this client is not client 3 in which case it already has more recent velocity info
					if (clientNumber != 3)
					{
						GameObject.FindGameObjectWithTag ("Player3").GetComponent<Player3Script>().xVelocity = Convert.ToSingle (tokens[2]);
						GameObject.FindGameObjectWithTag ("Player3").GetComponent<Player3Script>().yVelocity = Convert.ToSingle (tokens[3]);
					}
				}

				//set player 4's velocity...
				else if(tokens[1].Equals("4"))
				{
					//...if this client is not client 4 in which case it already has more recent velocity info
					if (clientNumber != 4)
					{
						GameObject.FindGameObjectWithTag ("Player4").GetComponent<Player4Script>().xVelocity = Convert.ToSingle (tokens[2]);
						GameObject.FindGameObjectWithTag ("Player4").GetComponent<Player4Script>().yVelocity = Convert.ToSingle (tokens[3]);
					}
				}
			}

			//used to initially create the pellets in game
			else if (tokens[0].Equals("spawnPellet"))
			{
				//this quaternion is used to make sure the player's avatar is correctly oriented
				Quaternion facingUp = new Quaternion(0,0,0,1);

				//spawns a pellet from the pellet prefab in the resources folder
				GameObject pellet = (GameObject)GameObject.Instantiate(Resources.Load ("Pellet"), 
				                       new Vector3(Convert.ToSingle(tokens[1]), Convert.ToSingle (tokens[2]),0f),
				                       facingUp);

				//sets the pellet id so that the clients know which pellet to replace upon collision
				pellet.GetComponent<PelletScript>().id = Int32.Parse (tokens[3]);
			}

			//used to "respawn" a pellet by repositioning it
			//respawnPellet\\newXPosition\\newYPosition\\IDofPelletToBeRepositioned
			else if (tokens[0].Equals("respawnPellet"))
			{
				//Get all the pellets in the game
				GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");
				int pelletTargetID = Int32.Parse(tokens[3]);

				//iterate through the list... 
				for (int i = 0; i < pellets.Length; i++)
				{
					//...to find the pellet with matching ID from the packet
					if (pellets[i].GetComponent<PelletScript>().id == pelletTargetID)
					{
						GameObject.Destroy(pellets[i]);
						GameObject newPellet = 
							(GameObject)GameObject.Instantiate(Resources.Load("Pellet"),new Vector3(
										Convert.ToSingle(tokens[1]), Convert.ToSingle(tokens[2]),0f),
							            new Quaternion(0,0,0,1));

						newPellet.GetComponent<PelletScript>().id = Int32.Parse (tokens[3]);
						break;
						//reposition the correct pellet and break from the loop
						//pellets[i].transform.position = 
						//	new Vector3(Convert.ToSingle(tokens[1]), Convert.ToSingle (tokens[2]),0f);
					
					}
				}
			}

			else if (tokens[0].Equals("winningClient"))
			{
				GameObject.Find("GameGUI").GetComponent<GameGUIScript>().guiText.text = "Player " + tokens[1] + " wins!";
				play = false;
				GameObject.Find("GameGUI").GetComponent<GameGUIScript>().showLogout = true;
			}

			//moves a specific player's avatar to its respective starting position
			//resetPlayer\\clientNumber
			else if (tokens[0].Equals("resetPlayer"))
			{
				GameObject.FindGameObjectWithTag("Player" + Int32.Parse(tokens[1])).transform.localScale = new Vector3 (3f, 3f, 0f);

				if(tokens[1].Equals("1"))
				{
					GameObject.FindGameObjectWithTag("Player1").GetComponent<Player1Script>().weight = 1;
					GameObject.FindGameObjectWithTag("Player1").transform.position = 
						new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0f);
				}
				
				else if(tokens[1].Equals("2"))
				{
					GameObject.FindGameObjectWithTag("Player2").GetComponent<Player2Script>().weight = 1;	
					GameObject.FindGameObjectWithTag("Player2").transform.position = 
						new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0f);
				}

				else if(tokens[1].Equals("3"))
				{
					GameObject.FindGameObjectWithTag("Player3").GetComponent<Player3Script>().weight = 1;	
					GameObject.FindGameObjectWithTag("Player3").transform.position = 
						new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0f);
				}

				else if(tokens[1].Equals("4"))
				{
					GameObject.FindGameObjectWithTag("Player4").GetComponent<Player1Script>().weight = 1;	
					GameObject.FindGameObjectWithTag("Player4").transform.position = 
						new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0f);
				}
			}

			//update an avatar's weight (increasing scale and effectively decreasing speed)
			else if (tokens[0].Equals("weight"))
			{
				int delta;
				Vector3 oldScale;

				switch (Int32.Parse (tokens[1]))
				{
					case 1:
						delta = Int32.Parse (tokens[2]);
						GameObject.FindGameObjectWithTag ("Player1").GetComponent<Player1Script>().weight += delta;
						oldScale = GameObject.FindGameObjectWithTag ("Player1").transform.localScale;
						GameObject.FindGameObjectWithTag ("Player1").transform.localScale = 
							new Vector3(oldScale.x + delta, oldScale.y + delta, oldScale.z);
						break;

					case 2:
						delta = Int32.Parse (tokens[2]);
						GameObject.FindGameObjectWithTag ("Player2").GetComponent<Player2Script>().weight += delta;
						oldScale = GameObject.FindGameObjectWithTag ("Player2").transform.localScale;
						GameObject.FindGameObjectWithTag ("Player2").transform.localScale = 
							new Vector3(oldScale.x + delta, oldScale.y + delta, oldScale.z);
						break;
//				case 3:
//					delta = Int32.Parse (tokens[2]);
//					GameObject.FindGameObjectWithTag ("Player3").GetComponent<Player1Script>().weight += delta;
//					oldScale = GameObject.FindGameObjectWithTag ("Player3").transform.localScale;
//					GameObject.FindGameObjectWithTag ("Player3").transform.localScale = 
//						new Vector3(oldScale.x + delta, oldScale.y + delta, oldScale.z);
//					break;
//				case 4:
//					delta = Int32.Parse (tokens[2]);
//					GameObject.FindGameObjectWithTag ("Player4").GetComponent<Player1Script>().weight += delta;
//					oldScale = GameObject.FindGameObjectWithTag ("Player4").transform.localScale;
//					GameObject.FindGameObjectWithTag ("Player4").transform.localScale = 
//						new Vector3(oldScale.x + delta, oldScale.y + delta, oldScale.z);
//					break;	
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
//			}

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
