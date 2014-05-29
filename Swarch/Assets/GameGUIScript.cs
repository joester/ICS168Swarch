using UnityEngine;
using System.Collections;

public class GameGUIScript : MonoBehaviour {

	public bool showStart;
	public bool showLogout;
	public GUIText guiText;
	GameProcess gp;

	// Use this for initialization
	void Start () {
		guiText.text = "";
		showStart = true;
		showLogout = true;
		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>();
	}

	void OnGUI () {

		//only show the start button if this boolean is true
		if(showStart)
		{
			if(GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 75, 50, 20), "Start"))
			{
				gp.returnSocket().SendTCPPacket("play");

				guiText.text = "Waiting for other players who have connected to press start...";
				showStart = false;
			}
		}
	
		//only show the logout button if this boolean is true
		if(showLogout)
		{
			if ( GUI.Button( new Rect( Screen.width / 2, Screen.height / 2 - 75, 80, 20), "Logout"))
			{			
				//send a disconnect packet
				gp.returnSocket().SendTCPPacket("logout\\" + gp.playerName);

				//keep the gameprocess object intact and return to main menu (level 0)
				DontDestroyOnLoad(gp);
				Application.LoadLevel(0);

				// KILL THREAD AND SERVER CONNECTION
				gp.returnSocket().t.Abort();
				gp.returnSocket().endThread();
				gp.returnSocket().Disconnect();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
