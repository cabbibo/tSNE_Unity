using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetNewClips : MonoBehaviour {

	public GameObject Selector;
	public pointBufferUpdater updater;
	public DisplayInfo metadata;
	public GameObject clipPrefab;
    public tSNEAudio audio;

    public GameObject selectHand;
    public Transform largeSpace;

	public GameObject[] clips;
	public int maxClips;
	public int currentClip = 0;

    public bool active = false;
    private float oLength = 0;

    public AudioClip onClip;
    public AudioClip offClip;

    private AudioSource audioOnOff;
    private MeshRenderer mesh;
	// Use this for initialization
	void Start () {
		EventManager.OnTriggerUp += OnTriggerUp;
		clips = new GameObject[ maxClips ];

        mesh = GetComponent<MeshRenderer>();    //EventManager.OnTriggerUp += OnTriggerUp;
        audioOnOff = GetComponent<AudioSource>();
	}

	void OnTriggerUp( GameObject g ){

        if( g == selectHand ){

        float length = updater.values[0];
        if (active == true && length < .1f)
        {

            if (clips[currentClip]) { Destroy(clips[currentClip]); }

            clips[currentClip] = (GameObject)Instantiate(clipPrefab, Selector.transform.position, Quaternion.identity);

            int id = (int)updater.values[1];

            clips[currentClip].GetComponent<ClipInfo>().id = id;

            print(metadata.FullInfo[0]);

            clips[currentClip].GetComponent<ClipInfo>().title = metadata.FullInfo[id].name;
            clips[currentClip].GetComponent<ClipInfo>().ogPos = Selector.transform.position;
            clips[currentClip].GetComponent<ClipInfo>().user = metadata.FullInfo[id].username;
            clips[currentClip].GetComponent<ClipInfo>().description = metadata.FullInfo[id].description;
            clips[currentClip].GetComponent<ClipInfo>().tags = metadata.FullInfo[id].tags;
            clips[currentClip].GetComponent<ClipInfo>().date = metadata.FullInfo[id].created;

            clips[currentClip].transform.parent = largeSpace;

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

	}
	
	// Update is called once per frame
	void Update () {

        float length = updater.values[0];
        if (active == true && length < .1f){
            mesh.enabled = true;
            transform.position = Selector.transform.position;
        }else{
            mesh.enabled = false;
        }
mesh.material.SetFloat( "_FullTime" , Time.time );
        if( length < .1f && oLength >= .1f ){
            //audioOnOff.clip = onClip;
            //audioOnOff.Play();

            mesh.material.SetFloat( "_HitTime" , Time.time );

        }else if( oLength < .1f && length >= .1f  ){

           //audioOnOff.clip = offClip;
           //audioOnOff.Play();

            mesh.material.SetFloat( "_HitTime" , Time.time );
        }

        oLength = length;

		
	}
}
