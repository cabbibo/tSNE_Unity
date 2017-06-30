using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchTargetBuffer : MonoBehaviour {

	public pointBufferUpdater points;
	public createCSVBuffers buffers;

	public int currentID = 1;
	public TextMesh text;


	// Use this for initialization
	void Start () {

    EventManager.OnTouchpadDown += OnTouchpadDown;
    EventManager.OnTouchpadUp += OnTouchpadUp;
    EventManager.StayTouchpad += StayTouchpad;

   

	}

	void OnTouchpadDown(GameObject o){
		currentID ++;
		currentID %= buffers.buffers.Length;
		Switch( currentID );

	}
	
	void OnTouchpadUp(GameObject o){

	}

	void StayTouchpad(GameObject o){

	}

	void Switch( int id ){


		currentID = id;
		points.SetTargetBuffer(buffers.buffers[id]);

		print( buffers.titles[id]);
		text.text = buffers.titles[id];

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
