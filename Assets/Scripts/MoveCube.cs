using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCube : MonoBehaviour {


	public GameObject movable;


	// The Order of sliders is going to be important!
	public PressButton[] Sliders;

	// Use this for initialization
	void Start () {

		for( int i = 0; i < Sliders.Length; i++ ){
			Sliders[i].WhileSliderDeltaPushed += WhileSliderDown;
			Sliders[i].OnButtonRelease += ButtonRelease;
		}
		
	}

	void WhileSliderDown( GameObject slider , Vector2 val){
		//print( val * 100 );

		
	}

	void ButtonRelease( GameObject slider ){

		Vector2 val = slider.GetComponent<PressButton>().dValueXY;
		Vector3 f = new Vector3( val.x , 0, val.y);
		movable.GetComponent<Rigidbody>().AddForce( f * 1000 );
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
