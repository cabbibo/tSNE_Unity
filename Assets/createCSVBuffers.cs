using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createCSVBuffers : MonoBehaviour {

	public TextAsset[] textValues;
	public string[] titles;
	public csvToBuffer[] buffers;


	// Use this for initialization
	void Start () {

		buffers = new csvToBuffer[textValues.Length];
		for( int i = 0; i < textValues.Length; i++ ){

			buffers[i] = gameObject.AddComponent<csvToBuffer>();
			buffers[i].textValues = textValues[i];
//			print( buffers[i].vertCount);
			buffers[i].Live();

		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
