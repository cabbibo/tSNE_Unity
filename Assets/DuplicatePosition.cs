using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicatePosition : MonoBehaviour {

    public GameObject toDuplicate;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        transform.position = toDuplicate.transform.position * transform.localScale.x;
		
	}
}
