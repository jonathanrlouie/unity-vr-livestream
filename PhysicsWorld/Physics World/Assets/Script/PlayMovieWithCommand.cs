using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMovieWithCommand : MonoBehaviour {
	// Update is called once per frame

	private SteamVR_TrackedObject trackedObj;
	private GameObject collidingObject;
	private GameObject objectInHand;
	public GameObject screen;
	private Renderer r;
	private MovieTexture movie;

	private SteamVR_Controller.Device Controller
	{
		get { return SteamVR_Controller.Input((int)trackedObj.index); }
	}

	private void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject>();
		r = screen.GetComponent<Renderer>();
		movie = (MovieTexture)r.material.mainTexture;
	}

	void Start(){
		movie.Stop ();
	}

	void Update () {
		if (Controller.GetHairTriggerDown()) {

			if (movie.isPlaying) {
				movie.Pause();
			}
			else {
				movie.Play();
			}
		}
	}
}
