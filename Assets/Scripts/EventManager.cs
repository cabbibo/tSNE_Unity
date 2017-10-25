using UnityEngine;
using System.Collections;
using Valve.VR;

public class EventManager : MonoBehaviour 
{

  public delegate void TriggerDown(GameObject t);
  public static event TriggerDown OnTriggerDown;

  public delegate void TriggerUp(GameObject t);
  public static event TriggerUp OnTriggerUp;

  public delegate void TriggerStay(GameObject t);
  public static event TriggerStay StayTrigger;

  public delegate void TouchpadDown(GameObject t);
  public static event TouchpadDown OnTouchpadDown;

  public delegate void TouchpadUp(GameObject t);
  public static event TouchpadUp OnTouchpadUp;


  public delegate void TouchpadHoverDown(GameObject t);
  public static event TouchpadHoverDown OnTouchpadHoverDown;

  public delegate void TouchpadHoverUp(GameObject t);
  public static event TouchpadHoverUp OnTouchpadHoverUp;

  public delegate void TouchpadStay(GameObject t);
  public static event TouchpadStay StayTouchpad;

  public delegate void GripDown(GameObject t);
  public static event GripDown OnGripDown;

  public delegate void GripUp(GameObject t);
  public static event GripUp OnGripUp;

  public delegate void GripStay(GameObject t);
  public static event GripStay StayGrip;

  public GameObject handL;
  public GameObject handR;

  SteamVR_TrackedObject trackedObjL;
  SteamVR_TrackedObject trackedObjR;

  void Start(){

    trackedObjL = handL.GetComponent<SteamVR_TrackedObject>();
    trackedObjR = handR.GetComponent<SteamVR_TrackedObject>();

  }
  
  void FixedUpdate(){

    getTrigger( handL , trackedObjL );
    getTrigger( handR , trackedObjR );

    getTouchpad( handL , trackedObjL );
    getTouchpad( handR , trackedObjR );

    getGrip( handL , trackedObjL );
    getGrip( handR , trackedObjR );

  }

  void getTrigger( GameObject go , SteamVR_TrackedObject tObj ){

    if((int) tObj.index < 0 ){ return; }
    var device = SteamVR_Controller.Input((int)tObj.index);

    if ( device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)){
      if(OnTriggerDown != null) OnTriggerDown(go);
    }

    if ( device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger)){
      if(OnTriggerUp != null) OnTriggerUp(go);
    }


    if ( device.GetTouch(SteamVR_Controller.ButtonMask.Trigger)){
      if(StayTrigger != null) StayTrigger(go);
    }

  }


  void getTouchpad( GameObject go , SteamVR_TrackedObject tObj ){

    if((int) tObj.index < 0 ){ return; }
    var device = SteamVR_Controller.Input((int)tObj.index);

    if ( device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)){
      if(OnTouchpadDown != null) OnTouchpadDown(go);
    }

    if ( device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad)){
      if(OnTouchpadUp != null) OnTouchpadUp(go);
    }


    if ( device.GetPress(SteamVR_Controller.ButtonMask.Touchpad)){
      if(StayTouchpad != null) StayTouchpad(go);
    }



    if ( device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad)){
      if(OnTouchpadHoverDown != null) OnTouchpadHoverDown(go);
    }

    if ( device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad)){
      if(OnTouchpadHoverUp != null) OnTouchpadHoverUp(go);
    }


  }

  void getGrip( GameObject go , SteamVR_TrackedObject tObj ){

    if((int) tObj.index < 0 ){ return; }
    var device = SteamVR_Controller.Input((int)tObj.index);

    if ( device.GetPressDown(SteamVR_Controller.ButtonMask.Grip)){
      if(OnGripDown != null) OnGripDown(go);
    }

    if ( device.GetPressUp(SteamVR_Controller.ButtonMask.Grip)){
      if(OnGripUp != null) OnGripUp(go);
    }


    if ( device.GetPress(SteamVR_Controller.ButtonMask.Grip)){
      if(StayGrip != null) StayGrip(go);
    }

  }



}