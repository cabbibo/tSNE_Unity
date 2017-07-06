using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchOnOff : MonoBehaviour {

    public PullCube mover;
    public DisplayInfo info;
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

            whileMoving.GetComponent<RenderToPoints>().active = true;
            //whileMoving.transform.localScale = new Vector3(1, 1, 1);
            whileStationary.GetComponent<RenderToPoints>().active = false;
            audio.updater = whileMoving.GetComponent<pointBufferUpdater>();
            info.updater = whileMoving.GetComponent<pointBufferUpdater>();

        } else
        {
            whileMoving.GetComponent<RenderClosest>().enabled = false;
            whileStationary.GetComponent<RenderClosest>().enabled = true;

            //whileMoving.transform.localScale = new Vector3(.3f, .3f, .3f);

            whileMoving.GetComponent<RenderToPoints>().active = false;
            whileStationary.GetComponent<RenderToPoints>().active = true;
            audio.updater = whileStationary.GetComponent<pointBufferUpdater>();
            info.updater = whileStationary.GetComponent<pointBufferUpdater>();

        }
	}
}
