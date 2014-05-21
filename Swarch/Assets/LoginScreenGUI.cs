using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;

public class LoginScreenGUI : MonoBehaviour {	
	

	public double delTime;
	public GUIText guiText;
	//public GUIText latencyText;
	public string userName;
	public string password;

	public GameProcess process;	
	private bool show;
	public bool connected;
	public long latency;
	
	void Start () 
	{
		show = false;
		connected = false;
		process = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		latency = -1;
		userName = "";
		password = "";
	}
	
	void OnGUI () {


		userName = GUI.TextField(new Rect(Screen.width / 2 - 55, Screen.height / 2 - 75, 125, 20), userName, 25);
		password = GUI.PasswordField(new Rect(Screen.width / 2 - 55, Screen.height / 2 - 50, 125, 20), password, '*', 25);
	
		userName = GUI.TextField(new Rect(Screen.width / 2 - 55, Screen.height / 2 - 75, 125, 20), userName, 30);
		password = GUI.PasswordField(new Rect(Screen.width / 2 - 55, Screen.height / 2 - 50, 125, 20), password, '*', 30);

			if(GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 20, 50, 20), "Login"))
			{
				if(!connected)
				{
					guiText.text = "Connecting...";
					if ( process.returnSocket().Connect() )
					{						
						//show = !show;
						guiText.text = "Connect Succeeded";	
						connected = true;
					}

					else 
						guiText.text = "Connect Failed";
				}
					//connect code
					string source = password;
					string hash;
					using (MD5 md5Hash = MD5.Create())
					{
						hash = GetMd5Hash(md5Hash, source);
						
						Console.WriteLine("The MD5 hash of " + source + " is: " + hash + ".");
						
						
						
						process.returnSocket().SendTCPPacket("userInfo\\" + userName + "\\" + hash);
					}
				}
				
			
		

//		if (!connected)
//		{
//			if (GUI.Button (new Rect (Screen.width / 2, Screen.height / 2 - 20,100,20), "Connect")) 
//			{
//				guiText.text = "Connecting...";
//				if ( process.returnSocket().Connect() )
//				{						
//					//show = !show;
//					guiText.text = "Connect Succeeded";	
//					connected = true;
//				}
//				
//				else guiText.text = "Connect Failed";
//			}
//		}	

//		if (connected)
//		{
////			if (GUI.Button( new Rect(Screen.width / 2 - 50, Screen.height / 2 + 100, 100, 20), "Test Latency"))
////			{
////				process.returnSocket().SendTCPPacket("lag\\" + process.clientNumber.ToString());
////			}
//
//			if ( GUI.Button( new Rect( Screen.width / 2, Screen.height / 2 - 20, 100, 20), "Disconnect"))
//			{
//				//********* COMPLETE THE FOLLOWING CODE
//				//********* KILL THREAD AND SEVER CONNECTION
//
//				guiText.text = "Disconnected.";
//				//show = !show;
//
//				connected = false;
//
//				process.returnSocket().t.Abort();
//				process.returnSocket().endThread();
//				process.returnSocket().Disconnect();
//			}
//		}
		
	
//		if (show && !process.play && !resetGame)
//		{
//			if (GUI.Button( new Rect(Screen.width / 2 - 200, Screen.height / 2 - 80, 80, 20), "Play"))
//			{
//				guiText.text = "Waiting for client " + getOtherClientNumber() + " to press Play";
//
//				//"play\\clientNumber" indicates that clientNumber is ready to start the game
//				process.returnSocket().SendTCPPacket("play\\" + process.clientNumber.ToString());
//			}
//		}	
//
//		if (resetGame)
//		{
//			guiText.text = "Player " + process.winningClientNumber + " wins!";
//
//			if (GUI.Button( new Rect(Screen.width / 2 - 200, Screen.height / 2 - 80, 80, 20), "Reset"))
//			{
//				show = true;
//				resetGame = false;
//				//Debug.Log (show + " " + process.play + " " + resetGame);
//				resetGuiText();
//				
//				GameObject.Find ("Player1Score").GetComponent<Player1ScoreScript>().resetScore();
//				GameObject.Find ("Player2Score").GetComponent<Player2ScoreScript>().resetScore();
//			}
//		}

		//if ( GUI.Button ( new Rect ( 500, 300, 100, 20 ), "Latency" ))
		//{
			//process.returnSocket().measureLatency() ;
		//}
		
		//GUI.Label ( new Rect ( 500, 330, 100, 20 ) , "Latency : " + process.returnSocket().returnLatency()  );
		
	}

	public void loginFail()
	{
		userName = "";
		password = "";
		guiText.text = "Invalid Login Info. Try Again.";
	}

	public void loginSucceed()
	{
		DontDestroyOnLoad(process);
		Application.LoadLevel(1);
	}

	string GetMd5Hash(MD5 md5Hash, string input)
	{
		
		// Convert the input string to a byte array and compute the hash. 
		byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
		
		// Create a new Stringbuilder to collect the bytes 
		// and create a string.
		StringBuilder sBuilder = new StringBuilder();
		
		// Loop through each byte of the hashed data  
		// and format each one as a hexadecimal string. 
		for (int i = 0; i < data.Length; i++)
		{
			sBuilder.Append(data[i].ToString("x2"));
		}
		
		// Return the hexadecimal string. 
		return sBuilder.ToString();
	}


	public void resetGuiText()
	{
		guiText.text = "";
	}

//	private int getOtherClientNumber()
//	{
//		return process.clientNumber == 1 ? 2 : 1;
//	}
	
	
	void Update () 
	{
	}
	
	public void printGui ( string printStr )
	{
		int wordCount = 0 ;
		string[] words = printStr.Split(' ');
		
		printStr = "";
		
		for ( int i = 0 ; i < words.Length ; ++ i )
		{
			if ( wordCount <= 4 )
			{
				printStr += words[i] + " " ;
				wordCount ++ ;
			}
			else
			{
				printStr += words[i] + "\n" ;
				wordCount = 0;
				
			}	
		}
		
		guiText.text = printStr ;
	}
	
	
	
}
