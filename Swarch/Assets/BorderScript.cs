using UnityEngine;
using System.Collections;

public class BorderScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.name.Equals("Player1"))
		{
			//GameObject.Find ("Player1").GetComponent<Player1Script>().reset ();
		}

		else if (coll.gameObject.name.Equals("Player2"))
		{
		//	GameObject.Find ("Player2").GetComponent<Player2Script>().reset ();
		}

		else
			Debug.Log ("error border coll");
	}
}
