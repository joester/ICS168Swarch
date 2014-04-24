using UnityEngine;
using System.Collections;

public class GameGUIScript : MonoBehaviour {


	public GUIText guiText;

	// Use this for initialization
	void Start () {
	
	}

	void OnGUI () {
		if(GUI.Button(new Rect(100, 130, 50, 20), "Start"))
		{
			guiText.text = "Waiting for other players to connect and press start.";
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
