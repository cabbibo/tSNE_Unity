using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetNewClips : MonoBehaviour {

	public GameObject Selector;
	public pointBufferUpdater updater;
	public DisplayInfo metadata;
	public GameObject clipPrefab;

	public GameObject[] clips;
	public int maxClips;
	public int currentClip = 0;

	// Use this for initialization
	void Start () {
		//EventManager.OnTriggerDown += OnTriggerDown;

		clips = new GameObject[ maxClips ];
    //EventManager.OnTriggerUp += OnTriggerUp;
	}

	void OnTriggerDown( GameObject g ){

		if( clips[currentClip]){ Destroy( clips[currentClip] ); }

		clips[currentClip] = (GameObject) Instantiate( clipPrefab , Selector.transform.position , Quaternion.identity );
		
		int id = (int)updater.values[1];
		
		clips[currentClip].GetComponent<ClipInfo>().id = id;

		print( metadata.FullInfo[0] );
		
		clips[currentClip].GetComponent<ClipInfo>().title = metadata.FullInfo[id].name;
		clips[currentClip].GetComponent<ClipInfo>().user = metadata.FullInfo[id].username;
		
		currentClip += 1;
		currentClip %= maxClips;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
