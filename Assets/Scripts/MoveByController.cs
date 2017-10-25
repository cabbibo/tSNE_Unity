using UnityEngine;
using System.Collections;

public class MoveByController : MonoBehaviour {


  public Transform ogTransform;
  public bool moving;
  public bool maintainVelocity;
  public bool maintainRotation;

  public bool inside;
  private Vector3 oPos;
  private Vector3[] posArray = new Vector3[3];
  private Vector3 vel;

  private Quaternion relQuat;
  private Vector3 relPos;

  public GameObject insideGO;
  public GameObject secondInsideGO;
  public GameObject movingController;

  public delegate void Release(GameObject t);
  public event Release OnRelease;

  public delegate void Grab(GameObject t);
  public event Grab OnGrab;

//  public MoveAudio moveAudio;


  Collider colInside;

	void OnEnable(){

    EventManager.OnTriggerDown += OnTriggerDown;
    EventManager.OnTriggerUp += OnTriggerUp;
    EventManager.StayTrigger += StayTrigger;
    inside = false;
    moving = false;

    //posArray = new Vector3[10];
  }

	// Update is called once per frame
	void FixedUpdate () {


   if( moving == true ){
      for( int i  = 2; i > 0; i --){
        posArray[i] = posArray[i-1];
      }



      posArray[0] = insideGO.transform.position;
     
     // vel = oPos - pos;
      transform.position = insideGO.transform.position;
      transform.rotation = insideGO.transform.rotation * relQuat;

      transform.position = transform.position - ( insideGO.transform.rotation* relPos);
      //transform.rotation = transform.rotation * relQuat;
    }
	
	}

  public void Restart(){
    insideGO = null;
    secondInsideGO = null;
    inside = false;
    moving = false;
  }

  void OnTriggerDown(GameObject o){

    
    

    if( inside == true  ){

      if( insideGO == o.GetComponent<controllerInfo>().interactionTip ){

        //if( o.GetInstanceID() == insideGO.transform.parent.GetInstanceID() ){
        //transform.SetParent(o.transform);
        moving = true;

        relPos = insideGO.transform.position - transform.position;

        relQuat = Quaternion.Inverse(insideGO.transform.rotation) * transform.rotation;
        relPos = Quaternion.Inverse(insideGO.transform.rotation) * relPos;

        GetComponent<Rigidbody>().isKinematic = true;
      //}

        movingController = o;



      }else if( secondInsideGO == o.GetComponent<controllerInfo>().interactionTip  ){

        GameObject tmp = insideGO;
        insideGO = secondInsideGO;
        secondInsideGO = tmp;


      //if( o.GetInstanceID() == insideGO.transform.parent.GetInstanceID() ){
      //transform.SetParent(o.transform);
      moving = true;

      relPos = insideGO.transform.position - transform.position;

      relQuat = Quaternion.Inverse(insideGO.transform.rotation) * transform.rotation;
      relPos = Quaternion.Inverse(insideGO.transform.rotation) * relPos;

      GetComponent<Rigidbody>().isKinematic = true;
    //}

      movingController = o;

      }

      Debug.Log("YO");
      if(OnGrab != null) OnGrab(transform.gameObject );

    }

  }

  void OnTriggerUp(GameObject o){
   //transform.SetParent(ogTransform);
    

    if( maintainVelocity == true && moving == true   ){

      for( int i = 0; i<2; i++){
        vel += ( posArray[i] - posArray[i+1] );
      }
      vel /= 3;
//      print( vel );
      GetComponent<Rigidbody>().velocity = vel * 200.0f;
      GetComponent<Rigidbody>().isKinematic = false; //= vel * 120.0f;

    }

    if( maintainRotation == true && moving == true ){

//      print( o.GetComponent<controllerInfo>().angularVelocity );
      GetComponent<Rigidbody>().angularVelocity = o.GetComponent<controllerInfo>().angularVelocity;
      GetComponent<Rigidbody>().isKinematic = false;
    }

    if( insideGO == o.GetComponent<controllerInfo>().interactionTip ){
      moving = false;
    }

    if( secondInsideGO != null && insideGO == o.GetComponent<controllerInfo>().interactionTip ){
      GameObject tmp = insideGO;
      insideGO = secondInsideGO;
      secondInsideGO = tmp;
    }

    if(OnRelease != null) OnRelease(transform.gameObject );
    
    
  }


  void StayTrigger(GameObject o){
//    print("ff");
  }


  void onCollisionEnter(){
    print( "check" );
  }



  void OnTriggerEnter(Collider Other){

  	//print("ya");
    if( Other.tag == "Hand" ){ 

    	//print("inset");
  
      inside = true; 
      //print( Other.gameObject );

     //if( moving == false ){
     //  insideGO = Other.gameObject;
     //}else{
        

      if( moving == false ){
        if( insideGO == null ){
          insideGO = Other.gameObject;
        }
      }

      if( insideGO != Other.gameObject ){
        secondInsideGO = Other.gameObject;
      }

      /*SteamVR_TrackedObject tObj = Other.transform.parent.transform.gameObject.GetComponent<SteamVR_TrackedObject>();
      var device = SteamVR_Controller.Input((int)tObj.index);
      device.TriggerHapticPulse(1000);*/
      
      //}
      //print( insideGO );
    }
  }

  void OnTriggerExit(Collider Other){
    if( Other.tag == "Hand" ){ 


    print("exisst");
      
      if( Other.gameObject == insideGO ){
        if( moving == false ){
          if( secondInsideGO == null ){
            inside = false;
            insideGO = null;
          }else{
            //inside = false;
            insideGO = secondInsideGO;
            secondInsideGO = null;

          }
        }
      }

    

      if( Other.gameObject == secondInsideGO ){
        secondInsideGO = null;
      }
    }
  }

  void OnTriggerStay( Collider Other ){
    if( Other.tag == "Hand" ){ 
     //SteamVR_TrackedObject tObj = Other.transform.parent.transform.gameObject.GetComponent<SteamVR_TrackedObject>();
      //var device = SteamVR_Controller.Input((int)tObj.index);
      //device.TriggerHapticPulse(30);
    }
  }

}