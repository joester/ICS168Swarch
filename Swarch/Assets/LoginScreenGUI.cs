//AFSHIN MAHINI 2013 - 2014

using UnityEngine;
using System.Collections;
using System.IO;

public class LoginScreenGUI : MonoBehaviour {	
	

	public double delTime;
	public GUIText guiText;
	public GUIText latencyText;
	public string userName;
	public string password;

	public GameProcess process;	
	private bool show;
	private bool connected;
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

		if (connected)
			if(GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 20, 50, 20), "Login"))
			{
				GameObject gp = (GameObject)Resources.Load ("GameProcess", typeof(GameObject));
				gp.GetComponent<GameProcess>().playerName = userName;
				Application.LoadLevel(1);
			}

		if (!connected)
		{
			if (GUI.Button (new Rect (Screen.width / 2, Screen.height / 2,100,20), "Connect")) 
			{
				guiText.text = "Connecting...";
				if ( process.returnSocket().Connect() )
				{						
					//show = !show;
					guiText.text = "Connect Succeeded";	
					connected = true;
				}
				
				else guiText.text = "Connect Failed";
			}
		}	

		if (connected)
		{
			if (GUI.Button( new Rect(Screen.width / 2 - 50, Screen.height / 2 + 100, 100, 20), "Test Latency"))
			{
				process.returnSocket().SendTCPPacket("lag\\" + process.clientNumber.ToString());
			}

			if ( GUI.Button( new Rect( Screen.width / 2, Screen.height / 2, 100, 20), "Disconnect"))
			{
				//********* COMPLETE THE FOLLOWING CODE
				//********* KILL THREAD AND SEVER CONNECTION

				guiText.text = "Disconnected.";
				//show = !show;

				connected = false;

				process.returnSocket().t.Abort();
				process.returnSocket().endThread();
				process.returnSocket().Disconnect();
			}
		}
		
	
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
