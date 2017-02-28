using UnityEngine;
using System.Collections;
using UnityEngine.Audio; // required for dealing with audiomixers

[RequireComponent(typeof(AudioSource))]
public class MicrophoneListener : MonoBehaviour {

	//Written in part by Benjamin Outram

	//option to toggle the microphone listenter on startup or not
	public bool startMicOnStartup = true;

	//allows start and stop of listener at run time within the unity editor

	private bool microphoneListenerOn = false;
	public bool disableOutputSound = false; 
	AudioSource src;

	public AudioMixer masterMixer;

	[SerializeField]
	KeyCode toggleToTalk = KeyCode.O;

	[SerializeField]
	KeyCode pushToTalk = KeyCode.P;

	public KeyCode PushToTalkKey {
		get { return pushToTalk; }
		set { pushToTalk = value; }
	}

	public KeyCode ToggleToTalkKey {
		get { return toggleToTalk; }
		set { toggleToTalk = value; }
	}

	public bool stopMicrophoneListener = false;
	public bool startMicrophoneListener =false;
	float timeSinceRestart = 0;

	void Start() {        
		//start the microphone listener
		if (startMicOnStartup) {
			//Debug.Log ("Hey pressed");
			RestartMicrophoneListener ();
			StartMicrophoneListener ();
		}
	}

	void Update(){    

		//can use these variables that appear in the inspector, or can call the public functions directly from other scripts
		if (stopMicrophoneListener) {
			StopMicrophoneListener ();
		}
		if (startMicrophoneListener) {
			StartMicrophoneListener ();
		}
		//reset paramters to false because only want to execute once
		stopMicrophoneListener = false;
		startMicrophoneListener = false;

		//must run in update otherwise it doesnt seem to work
		MicrophoneIntoAudioSource (microphoneListenerOn);

		//can choose to unmute sound from inspector if desired
		DisableSound (!disableOutputSound);
	}


	//stops everything and returns audioclip to null
	public void StopMicrophoneListener(){
		//stop the microphone listener
		microphoneListenerOn = false;
		//reenable the master sound in mixer
		disableOutputSound = false;
		//remove mic from audiosource clip
		src.Stop ();
		src.clip = null;

		Microphone.End (null);
	}


	public void StartMicrophoneListener(){
		//start the microphone listener
			microphoneListenerOn = true;
			//disable sound output (dont want to hear mic input on the output!)
			disableOutputSound = true;
			//reset the audiosource
			RestartMicrophoneListener ();
	}


	//controls whether the volume is on or off, use "off" for mic input (dont want to hear your own voice input!) 
	//and "on" for music input
	public void DisableSound(bool SoundOn){

		float volume = 0;

		if (SoundOn) {
			volume = 0.0f;
		} else {
			volume = -80.0f;
		}

		masterMixer.SetFloat ("MasterVolume", volume);
	}



	// restart microphone removes the clip from the audiosource
	public void RestartMicrophoneListener(){
			src = GetComponent<AudioSource> ();

			//remove any soundfile in the audiosource
			src.clip = null;

			timeSinceRestart = Time.time;

	}

	//puts the mic into the audiosource
	void MicrophoneIntoAudioSource (bool MicrophoneListenerOn){

		if(MicrophoneListenerOn){
			//pause a little before setting clip to avoid lag and bugginess
			if (Time.time - timeSinceRestart > 0.5f && !Microphone.IsRecording (null)) {
				src.clip = Microphone.Start (null, true, 10, 44100);

				//wait until microphone position is found (?)
				while (!(Microphone.GetPosition (null) > 0)) {
				}

				src.Play (); // Play the audio source
			}
		}
	}

}