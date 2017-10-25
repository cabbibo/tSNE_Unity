using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createCSVBuffers : MonoBehaviour {

	public TextAsset[] textValues;
	public string[] titles;
	public csvToBuffer[] buffers;

	public int id1;
	public int id2;
	public int id3;


	// Use this for initialization
	void Start () {

		buffers = new csvToBuffer[textValues.Length];
		for( int i = 0; i < textValues.Length; i++ ){

			buffers[i] = gameObject.AddComponent<csvToBuffer>();
			buffers[i].id1 = id1;
			buffers[i].id2 = id2;
			buffers[i].id3 = id3;
			buffers[i].textValues = textValues[i];
			buffers[i].Live();

		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
