using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipInfo : MonoBehaviour {

	public GameObject eye;
	public int id;
	public string title;
	public string user;

	public GameObject XY;

    public AudioClip largeClip;
    public float playTime;

    public string description;
    public string date;
    public string[] tags;

	public TextMesh titleMesh;
	public TextMesh userMesh;
    public TextMesh descriptionMesh;
    public TextMesh dateMesh;
    public TextMesh tagMesh;

    public Vector3 ogPos;

    public AudioSource source;

    private float rate = .5f;
    private float pitch = .5f;
    private float timeSinceLastPlayed = 0;
    private bool playing = false;

    private Material mat;
    void WhileSliderXYPushed( GameObject b , Vector2 value ){

    	rate = value.y;
    	pitch = value.x;

   }


	// Use this for initialization
	void Start () {
        source = GetComponent<AudioSource>();		
        XY.GetComponent<PressButton>().WhileSliderXYPushed += WhileSliderXYPushed;

        mat = GetComponent<MeshRenderer>().material;//.SetColor("_Color", Color.red);
	}
	
	// Update is called once per frame
	void FixedUpdate (){

		titleMesh.text = title;
		userMesh.text = user;
        descriptionMesh.text = description;
        dateMesh.text = date;
        //tagMesh.text = tags[0];

		transform.LookAt( eye.transform.position );

		if( playing == true){
			timeSinceLastPlayed += Time.deltaTime;
			if( timeSinceLastPlayed > rate ){
				timeSinceLastPlayed -= rate;
				Play();
			}
		}
		
	}

    public void Play()
    {
        source.clip = largeClip;
        source.time = playTime;
       // source.pitch = pitch * 2;
        source.Play();
        source.SetScheduledEndTime(AudioSettings.dspTime + (.25f ));
        mat.SetFloat("_LastPlayTime", Time.time);
    }

    void OnTriggerEnter( Collider C)
    {
        Toggle(); Play(); //transform.position = ogPos; GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    void OnCollisionEnter( Collision c)
    {
        //Toggle(); Play(); //transform.position = ogPos; GetComponent<Rigidbody>().velocity = Vector3.zero;

    }

    void Toggle(){

    	if( playing == true ){
    		mat.SetColor("_Color", Color.red);
    		playing = false;
    	}else{
    		mat.SetColor("_Color", Color.green);
    		playing = true;
    		timeSinceLastPlayed = 0;
    	}

    }
}
