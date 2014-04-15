using UnityEngine;
using System.Collections;

public class LoginScreen : MonoBehaviour {
	public string userName;
	public string password;

	// Use this for initialization
	void Start () {
		userName = "";
		password = "";
	}

	void OnGUI()
	{
		userName = GUI.TextField(new Rect(95, 75, 125, 20), userName, 25);
		password = GUI.PasswordField(new Rect(95, 100, 125, 20), password, '*', 25);

		if(GUI.Button(new Rect(100, 130, 50, 20), "Login"))
		{
			GameObject gp = (GameObject)Resources.Load ("GameProcess", typeof(GameObject));
			gp.GetComponent<GameProcess>().playerName = userName;
			Application.LoadLevel(1);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
