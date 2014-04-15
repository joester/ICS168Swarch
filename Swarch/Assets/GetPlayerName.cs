using UnityEngine;
using System.Collections;

public class GetPlayerName : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		this.guiText.text = GameObject.Find("GameProcess").GetComponent<GameProcess>().playerName;
	}
}
