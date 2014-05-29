using UnityEngine;
using System.Collections;

public class GetIPScript : MonoBehaviour {

	GameProcess gp;

	// Use this for initialization
	void Start () {
		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>();
	}
	
	// Update is called once per frame
	void Update () {
		//get the current IP from the Sockets class
		this.guiText.text = "Current IP: " + gp.returnSocket().getIP();
	}
}
