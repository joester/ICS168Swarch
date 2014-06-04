using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;

public class GameProcess : MonoBehaviour {

	//PUBLIC MEMBERS 
	public int clientNumber;
	public string playerName;
	public bool play;

	public DateTime dT;
	public Stopwatch uniClock;

	//PRIVATE MEMBERS
	private Sockets socks;
	private string stringBuffer;
	private string tempBuffer;

	// Use this for initialization
	void Start () {

		uniClock = new Stopwatch();

		//play = false;
		socks = new Sockets();

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

			//alreadyLoggedIn\\username
			else if (tokens[0].Equals("alreadyLoggedIn"))
			{
				GameObject.Find("Login_GUI").GetComponent<LoginScreenGUI>().guiText.text =
					tokens[1] + " is already logged in!";
			}

			//connected\\clientNumberThatConnected
			else if (tokens[0].Equals("connected"))
			{
				//this quaternion is used to make sure the player's avatar is correctly oriented
				Quaternion facingUp = new Quaternion(0,0,0,1);

				int clientThatJustConnected = Int32.Parse (tokens[1]);
				//UnityEngine.Debug.Log (clientNumber + " was told to spawn " + clientThatJustConnected);

				//spawns a player avatar corresponding to the information from the packet (see above)
				//also attaches a playerWeight GUIText to that player
				switch (clientThatJustConnected)
				{
				case 1:
					GameObject.Instantiate(Resources.Load ("Player1"), new Vector3(-2.5f,0f,0f), facingUp);
					GameObject.Instantiate(Resources.Load ("Player1Weight"), new Vector3((-2.5f + 5f) / 10f,0f,0f), facingUp);
					break;
				case 2:
					GameObject.Instantiate(Resources.Load ("Player2"), new Vector3(0f,2.5f,0f), facingUp);
					GameObject.Instantiate(Resources.Load ("Player2Weight"), new Vector3(0f,(2.5f + 5f) / 10f,0f), facingUp);
					break;
				case 3:
					GameObject.Instantiate(Resources.Load ("Player3"), new Vector3(2.5f,0f,0f), facingUp);
					GameObject.Instantiate(Resources.Load ("Player3Weight"), new Vector3((2.5f + 5f) / 10f,0f,0f), facingUp);
					break;
				case 4:
					GameObject.Instantiate(Resources.Load ("Player4"), new Vector3(0f,-2.5f,0f), facingUp);
					GameObject.Instantiate(Resources.Load ("Player4Weight"), new Vector3(0f,(-2.5f + 5f) / 10f,0f), facingUp);
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
						GameObject.FindGameObjectWithTag("Player1").GetComponent<Player1Script>().xVelocity = 
							Convert.ToSingle (tokens[2]);
						GameObject.FindGameObjectWithTag("Player1").GetComponent<Player1Script>().yVelocity = 
							Convert.ToSingle (tokens[3]);
					}
				}

				//set player 2's velocity...
				else if(tokens[1].Equals("2"))
				{
					//...if this client is not client 2 in which case it already has more recent velocity info
					if (clientNumber != 2)
					{
						GameObject.FindGameObjectWithTag ("Player2").GetComponent<Player2Script>().xVelocity = 
							Convert.ToSingle (tokens[2]);
						GameObject.FindGameObjectWithTag ("Player2").GetComponent<Player2Script>().yVelocity = 
							Convert.ToSingle (tokens[3]);
					}
				}

				//set player 3's velocity...
				else if(tokens[1].Equals("3"))
				{
					//...if this client is not client 3 in which case it already has more recent velocity info
					if (clientNumber != 3)
					{
						GameObject.FindGameObjectWithTag ("Player3").GetComponent<Player3Script>().xVelocity = 
							Convert.ToSingle (tokens[2]);
						GameObject.FindGameObjectWithTag ("Player3").GetComponent<Player3Script>().yVelocity = 
							Convert.ToSingle (tokens[3]);
					}
				}

				//set player 4's velocity...
				else if(tokens[1].Equals("4"))
				{
					//...if this client is not client 4 in which case it already has more recent velocity info
					if (clientNumber != 4)
					{
						GameObject.FindGameObjectWithTag ("Player4").GetComponent<Player4Script>().xVelocity = 
							Convert.ToSingle (tokens[2]);
						GameObject.FindGameObjectWithTag ("Player4").GetComponent<Player4Script>().yVelocity =
							Convert.ToSingle (tokens[3]);
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
						//To respawn the pellet: First destroy the pellet that got eaten...
						GameObject.Destroy(pellets[i]);

						//Then make a new pellet at the location received in the packet
						GameObject newPellet = 
							(GameObject)GameObject.Instantiate(Resources.Load("Pellet"),new Vector3(
										Convert.ToSingle(tokens[1]), Convert.ToSingle(tokens[2]),0f),
							            new Quaternion(0,0,0,1));

						//set the ID of the newly spawned pellet to the ID that was received
						newPellet.GetComponent<PelletScript>().id = Int32.Parse (tokens[3]);

						//break from the loop as soon as the pellet is found
						break;
					}
				}
			}

			//winningClient\\winningClientNumber
			else if (tokens[0].Equals("winningClient"))
			{
				if (tokens[1].Equals(playerName))
				{
					GameObject.Find("WinnerGUI").guiText.text = "You win!";
				}

				else
				{
					//set the text on screen to indicate to all players which client won
					GameObject.Find("WinnerGUI").guiText.text = tokens[1] + " wins!";
				}

				//remove the ability of the players to move by setting the play variable to false
				play = false;

				//show the logout button to allow the player to logout and replay if desired
				GameObject.Find("GameGUI").GetComponent<GameGUIScript>().showLogout = true;
			}

			//moves a specific player's avatar to a random position and resets the scale to the default
			//resetPlayer\\clientNumber\\xPos\\yPos
			else if (tokens[0].Equals("resetPlayer"))
			{
				//changes player 1's avatar to a new random location and resets the weight to 1
				//Note: the weight is responsible for altering the velocity of the avatar
				if(tokens[1].Equals("1"))
				{
					GameObject.FindGameObjectWithTag("Player1").GetComponent<Player1Script>().weight = 1;
					GameObject.FindGameObjectWithTag("Player1").transform.position = 
						new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0f);
				}

				//changes player 2's avatar to a new random location and resets the weight to 1
				//Note: the weight is responsible for altering the velocity of the avatar
				else if(tokens[1].Equals("2"))
				{
					GameObject.FindGameObjectWithTag("Player2").GetComponent<Player2Script>().weight = 1;	
					GameObject.FindGameObjectWithTag("Player2").transform.position = 
						new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0f);
				}

				//changes player 3's avatar to a new random location and resets the weight to 1
				//Note: the weight is responsible for altering the velocity of the avatar
				else if(tokens[1].Equals("3"))
				{
					GameObject.FindGameObjectWithTag("Player3").GetComponent<Player3Script>().weight = 1;	
					GameObject.FindGameObjectWithTag("Player3").transform.position = 
						new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0f);
				}

				//changes player 4's avatar to a new random location and resets the weight to 1
				//Note: the weight is responsible for altering the velocity of the avatar
				else if(tokens[1].Equals("4"))
				{
					GameObject.FindGameObjectWithTag("Player4").GetComponent<Player4Script>().weight = 1;	
					GameObject.FindGameObjectWithTag("Player4").transform.position = 
						new Vector3(Convert.ToSingle(tokens[2]), Convert.ToSingle(tokens[3]), 0f);
				}

				//reset the scale of the client's avatar to the default: Vector3(3,3,0)
				GameObject.FindGameObjectWithTag("Player" + Int32.Parse(tokens[1])).transform.localScale = new Vector3 (3f, 3f, 0f);
			}

			//update an avatar's weight (increasing scale and effectively decreasing speed)
			//weight\\deltaWeight (the amount to add to current weight)
			else if (tokens[0].Equals("weight"))
			{
				int delta;
				Vector3 oldScale;

				//determining which player's weight got altered...
				switch (Int32.Parse (tokens[1]))
				{
					case 1:
						//...and by how much...
						delta = Int32.Parse (tokens[2]);

						//...then adding the change in weight to the old weight to update it...
						GameObject.FindGameObjectWithTag ("Player1").GetComponent<Player1Script>().weight += delta;

						//...then updating the scale accordingly
						oldScale = GameObject.FindGameObjectWithTag ("Player1").transform.localScale;
						GameObject.FindGameObjectWithTag ("Player1").transform.localScale = 
							new Vector3(oldScale.x + delta, oldScale.y + delta, oldScale.z);
						break;

					case 2:
						//...and by how much...
						delta = Int32.Parse (tokens[2]);

						//...then adding the change in weight to the old weight to update it...
						GameObject.FindGameObjectWithTag ("Player2").GetComponent<Player2Script>().weight += delta;

						//...then updating the scale accordingly
						oldScale = GameObject.FindGameObjectWithTag ("Player2").transform.localScale;
						GameObject.FindGameObjectWithTag ("Player2").transform.localScale = 
							new Vector3(oldScale.x + delta, oldScale.y + delta, oldScale.z);
						break;

					case 3:
						//...and by how much...
						delta = Int32.Parse (tokens[2]);

						//...then adding the change in weight to the old weight to update it...
						GameObject.FindGameObjectWithTag ("Player3").GetComponent<Player3Script>().weight += delta;

						//...then updating the scale accordingly
						oldScale = GameObject.FindGameObjectWithTag ("Player3").transform.localScale;
						GameObject.FindGameObjectWithTag ("Player3").transform.localScale = 
							new Vector3(oldScale.x + delta, oldScale.y + delta, oldScale.z);
						break;

					case 4:
						//...and by how much...
						delta = Int32.Parse (tokens[2]);

						//...then adding the change in weight to the old weight to update it...
						GameObject.FindGameObjectWithTag ("Player4").GetComponent<Player4Script>().weight += delta;

						//...then updating the scale accordingly
						oldScale = GameObject.FindGameObjectWithTag ("Player4").transform.localScale;
						GameObject.FindGameObjectWithTag ("Player4").transform.localScale = 
							new Vector3(oldScale.x + delta, oldScale.y + delta, oldScale.z);
						break;	
				}
			}

			else if (tokens[0].Equals("disconnected"))
			{
				int clientThatJustDisconnected = Int32.Parse (tokens[1]);
				
				//destroys a player avatar corresponding to the information from the packet (see above)
				//also destroys the playerWeight GUIText of that player
				switch (clientThatJustDisconnected)
				{
				case 1:
					GameObject.Destroy(GameObject.FindGameObjectWithTag("Player1"));
					GameObject.Destroy(GameObject.FindGameObjectWithTag("Weight1"));
					break;
				case 2:
					GameObject.Destroy(GameObject.FindGameObjectWithTag("Player2"));
					GameObject.Destroy(GameObject.FindGameObjectWithTag("Weight2"));
					break;
				case 3:
					GameObject.Destroy(GameObject.FindGameObjectWithTag("Player3"));
					GameObject.Destroy(GameObject.FindGameObjectWithTag("Weight3"));
					break;
				case 4:
					GameObject.Destroy(GameObject.FindGameObjectWithTag("Player4"));
					GameObject.Destroy(GameObject.FindGameObjectWithTag("Weight4"));
					break;					
				}
			}

			else if (tokens[0].Equals("score"))
			{
				//determining which player's score got altered...
				switch (Int32.Parse (tokens[1]))
				{
					case 1:
					GameObject.FindGameObjectWithTag("Player1").GetComponent<Player1Script>().score = Int32.Parse(tokens[2]);
					break;

					case 2:
					GameObject.FindGameObjectWithTag("Player2").GetComponent<Player2Script>().score = Int32.Parse(tokens[2]);
					break;

					case 3:
					GameObject.FindGameObjectWithTag("Player3").GetComponent<Player3Script>().score = Int32.Parse(tokens[2]);
					break;

					case 4:
					GameObject.FindGameObjectWithTag("Player4").GetComponent<Player4Script>().score = Int32.Parse(tokens[2]);
					break;
				}
			}

			else
			{
				string packet = "Unrecognized packet: ";
				foreach (string i in tokens)
				{
					packet += i;

					if(i != tokens[tokens.Length - 1])
						packet += "\\";
				}
				UnityEngine.Debug.Log(packet);
			}
		}
	}

	public Sockets returnSocket ()
	{
		return socks;
	}
}
