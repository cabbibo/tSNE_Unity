using UnityEngine;
using System.Collections;

public class MoveTowardsObject : MonoBehaviour {


  public GameObject objectToMoveTowards;
  private Rigidbody Rigidbody;


  private Vector3 v1;

	// Use this for initialization
	void Start () {

    transform.position = objectToMoveTowards.transform.position;
    Rigidbody = GetComponent<Rigidbody>();
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {

      UpdatePosition();
	
	}

  private void UpdatePosition()
        {
            Rigidbody.maxAngularVelocity = float.MaxValue; 
            Quaternion RotationDelta;
            Vector3 PositionDelta;

            float angle;
            Vector3 axis;

            RotationDelta = objectToMoveTowards.transform.rotation * Quaternion.Inverse(transform.rotation);
            PositionDelta = (objectToMoveTowards.transform.position - transform.position);

            RotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (angle != 0)
            {
                Vector3 AngularTarget = angle * axis;
                this.Rigidbody.angularVelocity = AngularTarget;
            }

            Vector3 VelocityTarget = PositionDelta / Time.fixedDeltaTime;
            this.Rigidbody.velocity = VelocityTarget;
        }
}