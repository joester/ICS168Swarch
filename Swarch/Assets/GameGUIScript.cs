using UnityEngine;
using System.Collections;

public class GameGUIScript : MonoBehaviour {

	public bool show;
	public GUIText guiText;
	GameProcess gp;

	// Use this for initialization
	void Start () {
		guiText.text = "";
		show = true;
		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>();
	}

	void OnGUI () {
		if(show)
		{
			if(GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 75, 50, 20), "Start"))
			{
				gp.returnSocket().SendTCPPacket("play");

				guiText.text = gp.clientNumber + "  - Waiting for other players to connect and press start.";
			}

			if ( GUI.Button( new Rect( Screen.width / 2, Screen.height / 2 - 75, 100, 20), "Disconnect"))
			{
				//********* COMPLETE THE FOLLOWING CODE
				//********* KILL THREAD AND SEVER CONNECTION
				
				//guiText.text = "Disconnected.";

				DontDestroyOnLoad(gp);
				Application.LoadLevel(0);

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
