using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchOnOff : MonoBehaviour {

    public PullCube mover;
    public tSNEAudio audio;
    public GameObject whileMoving;
    public GameObject whileStationary;
	
    
    // Use this for initialization
	void Start () {
        mover = GetComponent<PullCube>();
	}
	
	// Update is called once per frame
	void Update () {
        if (mover.moving)
        {
            whileMoving.GetComponent<RenderClosest>().enabled = true;
            whileStationary.GetComponent<RenderClosest>().enabled = false;
            audio.updater = whileMoving.GetComponent<pointBufferUpdater>();

        } else
        {
            //whileMoving.GetComponent<RenderClosest>().enabled = false;
            //whileStationary.GetComponent<RenderClosest>().enabled = true;
            audio.updater = whileStationary.GetComponent<pointBufferUpdater>();

        }
	}
}
