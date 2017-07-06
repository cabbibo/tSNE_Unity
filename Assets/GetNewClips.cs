using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetNewClips : MonoBehaviour {

	public GameObject Selector;
	public pointBufferUpdater updater;
	public DisplayInfo metadata;
	public GameObject clipPrefab;
    public tSNEAudio audio;

	public GameObject[] clips;
	public int maxClips;
	public int currentClip = 0;

    public bool active = false;

	// Use this for initialization
	void Start () {
		EventManager.OnTriggerUp += OnTriggerUp;

		clips = new GameObject[ maxClips ];
    //EventManager.OnTriggerUp += OnTriggerUp;
	}

	void OnTriggerUp( GameObject g ){


        float length = updater.values[0];
        if (active == true && length < .02f)
        {

            if (clips[currentClip]) { Destroy(clips[currentClip]); }

            clips[currentClip] = (GameObject)Instantiate(clipPrefab, Selector.transform.position, Quaternion.identity);

            int id = (int)updater.values[1];

            clips[currentClip].GetComponent<ClipInfo>().id = id;

            print(metadata.FullInfo[0]);

            clips[currentClip].GetComponent<ClipInfo>().title = metadata.FullInfo[id].name;
            clips[currentClip].GetComponent<ClipInfo>().ogPos = Selector.transform.position;
            clips[currentClip].GetComponent<ClipInfo>().user = metadata.FullInfo[id].username;

            int whichClip = id / 4429;
            int remain = id - whichClip * 4429;

            AudioClip c = audio.clips[whichClip];


            float t = (float)remain / 4429;
            float time = c.length * t;


            clips[currentClip].GetComponent<ClipInfo>().largeClip = audio.clips[whichClip];
            clips[currentClip].GetComponent<ClipInfo>().playTime = time;
            

            currentClip += 1;
            currentClip %= maxClips;


           


            
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
