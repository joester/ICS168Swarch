using UnityEngine;
using System.Collections;

public class GetPlayerName : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	//Used to set the player's name in the game screen (from the input in the login screen)
	void Update () {
		//shows the player name in the game
		this.guiText.text = GameObject.Find("GameProcess").GetComponent<GameProcess>().playerName;
	}
}
