﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointBufferUpdater : MonoBehaviour {

	public csvToBuffer vertBuffer;
	public csvToBuffer targetBuffer;
	public ComputeShader physics;
	public ComputeShader gather;

	public GameObject Selector;
	public controllerInfo Hand;
	private int _kernel;
	private int _gatherKernel;


	private int numParticles;
	private int numGroups;


	public float[] floatValues;
	public float[] values;

	public ComputeBuffer _gatherBuffer;
	public ComputeBuffer _floatBuffer;

	public bool ready = false;


	// Use this for initialization
	void Start () {
		
		vertBuffer = GetComponent<csvToBuffer>();
		vertBuffer.Live();
		WhenBufferReady();

	}

		// Use this for initialization
	public void WhenBufferReady() {

		_kernel = physics.FindKernel("CSMain");
		_gatherKernel = gather.FindKernel("CSMain");

		numParticles =  vertBuffer.vertCount;
		int numThreads = 256;
		
		numGroups = (numParticles+(numThreads-1))/numThreads;

		floatValues = new float[4*numGroups];
		values = new float[4];

		_floatBuffer = new ComputeBuffer(numGroups, 4 * sizeof(float));
		_gatherBuffer = new ComputeBuffer(1, 4 * sizeof(float));
		

		ready = true;

	   	//physics.Dispatch( _kernel, vertBuffer.SIZE , vertBuffer.SIZE , vertBuffer.SIZE );
		
	}

public void SetTargetBuffer(csvToBuffer buffer){

	targetBuffer = buffer;
}
	
	// Update is called once per frame
	void FixedUpdate () {

		if( ready == true ){

			physics.SetFloat( "_DeltaTime"    , Time.deltaTime );
	    physics.SetFloat( "_Time"         , Time.time      );
	    physics.SetFloat( "_ClosestID"         , values[1]     );
	    physics.SetFloat( "_SelectorDown"      , Hand.triggerVal     );
	    physics.SetVector( "_SelectorPosition" , Selector.transform.position      );

			physics.SetBuffer( _kernel , "vertBuffer" , vertBuffer._buffer );
			
			if( targetBuffer != null ){
				physics.SetBuffer( _kernel , "targetBuffer" , targetBuffer._buffer );
				physics.SetInt( "_HasTarget" , 1 );
			}else{
				physics.SetInt( "_HasTarget" , 0 );
			}

			physics.SetBuffer( _kernel , "outBuffer" , _floatBuffer );

			physics.SetInt( "_NumVerts" , numParticles );
			physics.SetInt( "_NumGroups", numGroups );

		  physics.Dispatch( _kernel, numGroups , 1 , 1 );


		  gather.SetBuffer( _gatherKernel , "floatBuffer" , _floatBuffer );
			gather.SetBuffer( _gatherKernel , "gatherBuffer" , _gatherBuffer );
		
			gather.Dispatch( _gatherKernel, 1 , 1 , 1 );
		
			_gatherBuffer.GetData(values);

		}
			
	}


}