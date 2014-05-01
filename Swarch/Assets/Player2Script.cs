using UnityEngine;
using System.Collections;

public class Player2Script : MonoBehaviour {
	
	public float xVelocity;
	public float yVelocity;	
	public float currentXPosition;
	public float lastXPosition;
	public float currentYPosition;
	public float lastYPosition;
	public float threshold;
	
	public float weight;

	public GameProcess gp;
	
	// Use this for initialization
	void Start () {
		xVelocity = 0;
		yVelocity = 0;		
		currentXPosition = 2.5f;
		lastXPosition = 2.5f;
		
		currentYPosition = 0f;
		lastYPosition = 0f;
		
		threshold = 0.2f;
		weight = 1;
		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>();

		
		StartCoroutine ( SendDelay() );
	}
	
	// Update is called once per frame
	void Update () {
		
		if (gp.clientNumber == 2)
		{
			if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
				yVelocity = 0.1f;
			
			if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
				yVelocity = -0.1f;
			
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
				xVelocity = -0.1f;
			
			if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
				xVelocity = 0.1f;
			
			if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) ||
			    Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
				yVelocity = 0.0f;	
			
			if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) ||
			    Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
				xVelocity = 0.0f;
		}
	}
	
	void FixedUpdate()
	{
		if (gp.clientNumber == 2 && gp.play)
		{
			transform.Translate( new Vector3(xVelocity / weight, yVelocity / weight, 0));
			currentXPosition = transform.position.x;
			currentYPosition = transform.position.y;		
		}
	}
	
	IEnumerator SendDelay() {
		
		while ( true )
		{
			float delay = 0.5f;
			
			yield return new WaitForSeconds ( delay ) ;
			
			if (gp.clientNumber == 2)
			{
				if (Mathf.Abs(currentXPosition - lastXPosition) >= threshold)
				{				
					lastXPosition = currentXPosition;
					//gp.returnSocket().SendTCPPacket("paddle\\" + 2 + "\\" + currentPosition + "\\" + delay);
				}
				
				if (Mathf.Abs(currentYPosition - lastYPosition) >= threshold)
				{
					lastYPosition = currentYPosition;
					//gp.returnSocket().SendTCPPacket("paddle\\" + 2 + "\\" + currentPosition + "\\" + delay);
				}	
			}
		}
	}
	
	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.name.Equals("Player1"))
		{
			if (weight > coll.gameObject.GetComponent<Player1Script>().weight)
			{
				coll.gameObject.GetComponent<Player1Script>().reset();
				float enemyWeight = coll.gameObject.GetComponent<Player1Script>().weight;
				weight += enemyWeight;
				
				Vector3 values = transform.localScale;

				transform.localScale = 
					new Vector3(values.x + enemyWeight, values.y + enemyWeight, 1);

				coll.gameObject.GetComponent<Player1Script>().reset();
			}
			
			else if (weight == coll.gameObject.GetComponent<Player1Script>().weight)
			{
				reset();
				coll.gameObject.GetComponent<Player1Script>().reset();
			}
		}
	}
	
	public void reset()
	{
		weight = 1;
		transform.position = new Vector3(Random.Range(-4f,4f), Random.Range(-4f,4f), 0);
		transform.localScale = new Vector3(3f, 3f, 1f);
	}
}

