using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipInfo : MonoBehaviour {

	public GameObject eye;
	public int id;
	public string title;
	public string user;


	public TextMesh titleMesh;
	public TextMesh userMesh;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update (){

		titleMesh.text = title;
		userMesh.text = user;

		transform.LookAt( eye.transform.position );
		
	}
}
