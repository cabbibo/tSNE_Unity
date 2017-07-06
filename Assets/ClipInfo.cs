using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipInfo : MonoBehaviour {

	public GameObject eye;
	public int id;
	public string title;
	public string user;

    public AudioClip largeClip;
    public float playTime;

	public TextMesh titleMesh;
	public TextMesh userMesh;

    public Vector3 ogPos;

    public AudioSource source;


	// Use this for initialization
	void Start () {
        source = GetComponent<AudioSource>();		
	}
	
	// Update is called once per frame
	void Update (){

		titleMesh.text = title;
		userMesh.text = user;

		transform.LookAt( eye.transform.position );
		
	}

    public void Play()
    {
        source.clip = largeClip;
        source.time = playTime;

        source.Play();
        source.SetScheduledEndTime(AudioSettings.dspTime + (.25f));
    }

    void OnTriggerEnter( Collider C)
    {
        Play(); transform.position = ogPos; GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    void OnCollisionEnter( Collision c)
    {
        Play(); transform.position = ogPos; GetComponent<Rigidbody>().velocity = Vector3.zero;

    }
}
