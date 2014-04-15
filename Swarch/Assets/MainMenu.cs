using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	public string userName = "Enter name: ";
	public string password = "Enter password: ";

	void OnGUI()
	{
		userName = GUI.TextField(new Rect(200, 75, 200, 20), userName, 25);

		password = GUI.PasswordField(new Rect(200, 100, 200, 20), password, '*', 25);
		if(GUI.Button(new Rect(270, 130, 50, 20), "Login"))
		{
			Application.LoadLevel(1);
		}

	}

	void awake()
	{
		DontDestroyOnLoad(this);
	}

	public string getUserName()
	{
		return userName;
	}


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
