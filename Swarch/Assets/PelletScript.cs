using UnityEngine;
using System.Collections;

public class PelletScript : MonoBehaviour {


	float weight;
	float scaleModifier;
	private bool isQuitting;

	public int id = -1;

	// Use this for initialization
	void Start () {
		weight = .5f;
		scaleModifier = 7f;
		isQuitting = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
//		if (coll.gameObject.name.Equals("Player1"))
//		{
//			GameObject.Find ("Player1").GetComponent<Player1Script>().weight += weight;
//		}
//
//		else if (coll.gameObject.name.Equals("Player2"))
//		{
//			GameObject.Find ("Player2").GetComponent<Player2Script>().weight += weight;
//		}
//
//		else
//			Debug.Log("pellet collision error!");
//
//		Vector3 values = coll.gameObject.transform.localScale;
//		coll.gameObject.transform.localScale = 
//			new Vector3(values.x + weight * scaleModifier, values.y + weight * scaleModifier, 1);	
//		GameObject.Destroy(this.gameObject);
	}
	
	void OnApplicationQuit()
	{
		isQuitting = true;
	}
}
