
using UnityEngine;
using System.Collections;
using Normal.Realtime;

public class HandInfo : MonoBehaviour {


	public Structs.Hand hand;

	public Vector3 debug;

	public Vector3 velocity;
	public float trigger;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		hand.localToWorld = transform.localToWorldMatrix;
		hand.worldToLocal = transform.worldToLocalMatrix;
		hand.vel = velocity;
		hand.pos = transform.position;
		hand.trigger = GetComponent<Hand>().triggerPosition;
		hand.debug = debug;

	}

	
}