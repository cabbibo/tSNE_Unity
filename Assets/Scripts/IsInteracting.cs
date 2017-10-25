using UnityEngine;
using System.Collections;

public class IsInteracting : MonoBehaviour {

  public bool isInteracting = false;
  //private bool stayTrigger = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  void OnTriggerEnter( Collider c){
    isInteracting = true;
  }
  void OnTriggerExit(Collider c){
    isInteracting = false;
  }
}