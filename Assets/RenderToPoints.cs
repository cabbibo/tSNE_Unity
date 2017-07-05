using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderToPoints : MonoBehaviour {


   


	public Material m;
	public csvToBuffer  vBuf;
	public pointBufferUpdater updater;


	// Use this for initialization
	void Start () {

		vBuf = GetComponent<csvToBuffer>();
		updater = GetComponent<pointBufferUpdater>();
		
	}
	
	// Update is called once per frame
	void Update () {

		//print("hmm");
		
	}

	void OnRenderObject(){

		if( vBuf._buffer != null ){

			//print("ss");
			m.SetPass(0);

			m.SetBuffer( "_vertBuffer", vBuf._buffer );
			if( updater.ready == true ){
				m.SetFloat( "_ClosestID" , updater.values[1] );
			}

			//Graphics.DrawProcedural(MeshTopology.Points, vBuf.vertCount );
			Graphics.DrawProcedural(MeshTopology.Triangles, vBuf.vertCount * 6 );
		}


	}

}
