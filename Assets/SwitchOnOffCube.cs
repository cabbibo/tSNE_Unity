using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchOnOffCube : MonoBehaviour {

    //public PullCube mover;
    public DisplayInfo info;
    public tSNEAudio audio;
    public GetNewClips clips;
    public GameObject whileMoving;
    public GameObject whileStationary;

    public bool inside = false;
    public Vector3 activePos;
    public GameObject marker;


    // Use this for initialization
    void Start()
    {

        EventManager.OnTriggerDown += OnTriggerDown;
        EventManager.OnTriggerUp += OnTriggerUp;
        EventManager.StayTrigger += StayTrigger;



    }

    // Update is called once per frame
    void Update()
    {
        if (inside)
        {
            whileMoving.GetComponent<RenderClosest>().enabled = true;
            whileStationary.GetComponent<RenderClosest>().enabled = false;

            whileMoving.GetComponent<RenderToPoints>().active = true;
            //whileMoving.transform.localScale = new Vector3(1, 1, 1);
            whileStationary.GetComponent<RenderToPoints>().active = false;
            audio.updater = whileMoving.GetComponent<pointBufferUpdater>();
            info.updater = whileMoving.GetComponent<pointBufferUpdater>();
            clips.updater = whileMoving.GetComponent<pointBufferUpdater>();
            clips.active = false;

        }
        else
        {
            whileMoving.GetComponent<RenderClosest>().enabled = false;
            //whileStationary.GetComponent<RenderClosest>().enabled = true;

            //whileMoving.transform.localScale = new Vector3(.3f, .3f, .3f);

            whileMoving.GetComponent<RenderToPoints>().active = false;
            whileStationary.GetComponent<RenderToPoints>().active = true;
            audio.updater = whileStationary.GetComponent<pointBufferUpdater>();
            info.updater = whileStationary.GetComponent<pointBufferUpdater>();
            clips.updater = whileStationary.GetComponent<pointBufferUpdater>();
            clips.active = true;

        }
        
        Vector3 fPos = ((whileMoving.transform.position-activePos) / whileMoving.transform.localScale.x) * whileStationary.transform.localScale.x;

        float sL = whileStationary.transform.localScale.x;
        float sS = whileMoving.transform.localScale.x;

        Vector3 dir = marker.transform.localPosition - whileMoving.transform.localPosition ;
        whileStationary.transform.position = -dir *sL / sS;


    }


    void OnTriggerEnter(Collider o)
    {
        print("helllo");
        inside = true;
        transform.localScale *= 5;
    }

    void OnTriggerExit(Collider o)
    {
        inside = false;
        transform.localScale /= 5;
    }

    void OnTriggerDown( GameObject t)
    {
        if( inside == true)
        {
            activePos = t.transform.position;
            marker.transform.position = activePos;
        }
    }

    void OnTriggerUp(GameObject t)
    {
        if (inside == true)
        {
            activePos = t.transform.position;
            marker.transform.position = activePos;
        }
    }

    void StayTrigger(GameObject t)
    {

        if (inside == true)
        {
            activePos = t.transform.position;
            marker.transform.position = activePos;
        }

    }

    
}
