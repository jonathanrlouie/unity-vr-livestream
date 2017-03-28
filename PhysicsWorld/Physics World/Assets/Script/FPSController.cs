using UnityEngine;
using System.Collections;

public class FPSController : MonoBehaviour {

	public float speed = 10.0F;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked; //turn off cursor and keeps it inside GameWindow
	}
	
	// Update is called once per frame
	void Update () {
		float translation = Input.GetAxis ("Vertical") * speed;
		float straffe = Input.GetAxis ("Horizontal") * speed;
		translation *= Time.deltaTime;
		straffe *= Time.deltaTime;

		transform.Translate(straffe, 0, translation);

		if (Input.GetKeyDown ("escape"))
			Cursor.lockState = CursorLockMode.None;
	}
}
