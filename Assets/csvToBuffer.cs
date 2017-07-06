using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csvToBuffer : MonoBehaviour {

	public TextAsset textValues;
	private Vector3[] positions;
    private string[] names;

    public int id1;
    public int id2;
    public int id3;
    public int startIndex;

    public int nameID;

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

		vertCount = textSplit.Length-startIndex;

		positions = new Vector3[ vertCount  ];

        string[] names = textSplit[startIndex].Split(","[0]);
        for ( int i =0; i < names.Length; i++)
        {
            print(i); print(names[i]);
        }
		for( int i = 0; i < vertCount-startIndex; i++ ){
			string[] data=textSplit[i+startIndex].Split(","[0]);

            if (i < 10)
            {
                //print(data.Length);
                //print(data[id1]);
                //print(data[id2]);
                //print(data[id3]); 
                //				print( positions[i]);
            }

            float x = float.Parse(data[id1],System.Globalization.CultureInfo.InvariantCulture.NumberFormat); 
			float y = float.Parse(data[id2],System.Globalization.CultureInfo.InvariantCulture.NumberFormat); 
			float z = float.Parse(data[id3],System.Globalization.CultureInfo.InvariantCulture.NumberFormat); 
			Vector3 p = new Vector3(  x,y,z );

			positions[i] = p;
			
			

		}

	}
}
