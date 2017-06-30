using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tSNEAudio : MonoBehaviour {

	public int numSources;
	public AudioClip[] clips;
	private AudioSource[] sources;
	private int currentSource = 0;
	private float oldID=100000000;

	public pointBufferUpdater updater;
	// Use this for initialization
	void Start () {
		sources = new AudioSource[numSources];
		for( int i = 0; i < numSources; i++ ){

			sources[i] = gameObject.AddComponent<AudioSource>();

		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		float n = updater.values[1];

		if( n != oldID ){ playSound( (int)n ); }

		oldID = n;
		
	}

	void playSound( int id ){

		currentSource ++;
		currentSource %= numSources;

		int whichClip = id / 4429;
		int remain = id - whichClip * 4429;

		AudioClip c = clips[ whichClip ];


		float t = (float)remain/4429;
		float time = c.length * t;


		sources[currentSource].clip = c;
		sources[currentSource].time = time;

		sources[currentSource].Play();
		sources[currentSource].SetScheduledEndTime(AudioSettings.dspTime+(.25f));


	}


}
