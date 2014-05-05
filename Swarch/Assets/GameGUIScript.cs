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
			if(GUI.Button(new Rect(Screen.width / 2 - 55, Screen.height / 2 - 75, 50, 20), "Start"))
			{
				gp.returnSocket().SendTCPPacket("play");

				guiText.text = gp.clientNumber + "  - Waiting for other players to connect and press start.";
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
