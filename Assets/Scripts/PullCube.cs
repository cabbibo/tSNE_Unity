using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullCube : MonoBehaviour {


	public GameObject dragObject;
	public GameObject Movable;

	// same scale in diff dimensions for rn.
	public float scaleDif;

	public float distFromCenter;

	public Vector3 newDir;
	public Vector3 oldPos;
	public Vector3 moveDir;
	public bool moving = false;






	// Use this for initialization
	void Start () {

		dragObject.GetComponent<MoveByController>().OnRelease += OnRelease;
		dragObject.GetComponent<MoveByController>().OnGrab += OnGrab;

		scaleDif = Movable.transform.localScale.x / transform.localScale.x;
		oldPos = Movable.transform.position;
		newDir = new Vector3(0,0,.00001f);
		moveDir = new Vector3(0,0,0);




		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	void FixedUpdate(){
		Vector3 f = dragObject.transform.position - transform.position;
		dragObject.GetComponent<Rigidbody>().AddForce( -f * 300  );

		if( moving == false ){

			float lerpVal = (f.magnitude / newDir.magnitude);
			if( float.IsNaN( lerpVal ) ){
				lerpVal = 0;
			}


			Movable.transform.position = oldPos + moveDir * -(1-lerpVal);

		}
	}

	void OnRelease(GameObject obj ){

		print( "released");
		moving = false;


		newDir = dragObject.transform.position - transform.position;
		oldPos = Movable.transform.position;
		moveDir = newDir;


	}

	void OnGrab(GameObject obj ){
		print("grab");
		moving = true;

	}

}
