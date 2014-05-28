using UnityEngine;
using System.Collections;

public class Player1Script : MonoBehaviour {

	public float currentXPosition;
	public float lastXPosition;
	public float currentYPosition;
	public float lastYPosition;
	public float threshold;

	public float xVelocity;
	public float yVelocity;	
	public float currentXVelocity;
	public float lastXVelocity;
	public float currentYVelocity;
	public float lastYVelocity;

	public float weight;

	public GameProcess gp;

	// Use this for initialization
	void Start () {
		xVelocity = 0;
		yVelocity = 0;		
		currentXPosition = -2.5f;
		lastXPosition = -2.5f;

		currentXVelocity = 0.0f;
		lastXVelocity = 0.0f;

		currentYVelocity = 0.0f;
		lastYVelocity = 0.0f;

		currentYPosition = 0f;
		lastYPosition = 0f;

		threshold = 0.05f;
		weight = 1;
		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		StartCoroutine ( SendDelay() );
	}
	
	// Update is called once per frame
	void Update () {

		//Read inputs if the client running this code is client 1
		if (gp.clientNumber == 1)
		{
			//WASD for moving via changing the velocity
			if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
				yVelocity = 0.1f;
			
			if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
				yVelocity = -0.1f;

			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
				xVelocity = -0.1f;
			
			if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
				xVelocity = 0.1f;

			//Check to see if the key was released to stop that velocity
			if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) ||
			    Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
				yVelocity = 0.0f;	

			if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) ||
			    Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
				xVelocity = 0.0f;

			//save the velocities
			currentXVelocity = xVelocity;
			currentYVelocity = yVelocity;

			//if either of the velocities have changed, send the new velocity to the server
			if (currentXVelocity != lastXVelocity)
			{
				gp.returnSocket().SendTCPPacket("velocity\\x\\"+currentXVelocity);
				lastXVelocity = currentXVelocity;
			}

			if (currentYVelocity != lastYVelocity)
			{
				gp.returnSocket().SendTCPPacket("velocity\\y\\"+currentYVelocity);
				lastYVelocity = currentYVelocity;
			}
		}
	}

	void FixedUpdate()
	{
		//allow movement only once the game has started
		if (gp.play)
		{
			transform.Translate( new Vector3(xVelocity / weight, yVelocity / weight, 0));
			currentXPosition = transform.position.x;
			currentYPosition = transform.position.y;			
		}

		//Makes sure that the player's score text follows the player as it moves
		GameObject.FindGameObjectWithTag("Weight1").transform.position = 
			new Vector3((transform.position.x + 5f) / 10f, (transform.position.y + 5f) / 10f, 0f);
		GameObject.FindGameObjectWithTag("Weight1").guiText.text = weight + "";
	}

	//used for simulated delay where "delay" = the artificial delay in ms
	IEnumerator SendDelay() {
		
		while ( true )
		{
			float delay = 0.0f;
			
			yield return new WaitForSeconds ( delay ) ;

			//only send the position updates for player 1 from client 1
			if (gp.clientNumber == 1)
			{
				//check that the difference in position since the last update passes the threshold
				if (Mathf.Abs(currentXPosition - lastXPosition) >= threshold)
				{				 
					lastXPosition = currentXPosition;
					gp.returnSocket().SendTCPPacket("position\\x\\" + currentXPosition);
				}

				if (Mathf.Abs(currentYPosition - lastYPosition) >= threshold)
				{
					lastYPosition = currentYPosition;
					gp.returnSocket().SendTCPPacket("position\\y\\" + currentYPosition);
				}
			}			
		}
	}
}

