using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csvToBuffer : MonoBehaviour {

	public TextAsset textValues;
	public Vector3[] positions;

	private string text;

	struct Vert{
		public float   id;
	  public Vector3 pos;
	  public Vector3 vel;
	  public Vector3 targetPos;
	  public Vector3 debug;
	};
	
	private int vertStructSize = 1+3+3+3+3;
	public int vertCount;
	private float[] values;

	public ComputeBuffer _buffer;

	
	// Use this for initialization
	public void Live() {

		text = textValues.text;
//		print( text);
		split();
		CreateBuffer();
		
	}

	void CreateBuffer(){

		_buffer = new ComputeBuffer( vertCount , vertStructSize * sizeof(float) );
		values = new float[ vertStructSize * vertCount ];

		int index = 0;
		for( int i = 0; i < vertCount; i++ ){
			
			// id 
			values[ index++ ] = i;

			// positions
			values[ index++ ] = positions[i].x;
			values[ index++ ] = positions[i].y;
			values[ index++ ] = positions[i].z;

			// vel
			values[ index++ ] = 0;
			values[ index++ ] = 0;
			values[ index++ ] = 0;

			// targetPos
			values[ index++ ] = positions[i].x;
			values[ index++ ] = positions[i].y;
			values[ index++ ] = positions[i].z;

			// Debug
			values[ index++ ] = 0;
			values[ index++ ] = 1;
			values[ index++ ] = 0;

		}

		_buffer.SetData(values);
	}
	
	void split(){

		//var myArray = string.Split(params char[])' 

		string[] textSplit;
		textSplit = text.Split("\n"[0]);
//		print( textSplit[0]);

		vertCount = textSplit.Length;

		positions = new Vector3[ vertCount ];


		for( int i = 0; i < vertCount; i++ ){
			string[] data=textSplit[i].Split(","[0]);

			float x = float.Parse(data[0],System.Globalization.CultureInfo.InvariantCulture.NumberFormat); 
			float y = float.Parse(data[1],System.Globalization.CultureInfo.InvariantCulture.NumberFormat); 
			float z = float.Parse(data[2],System.Globalization.CultureInfo.InvariantCulture.NumberFormat); 
			Vector3 p = new Vector3(  x,y,z );

			positions[i] = p;
			
			if( i < 100 ){
//				print( data[0]);
//				print( positions[i]);
			}

		}

	}
}
