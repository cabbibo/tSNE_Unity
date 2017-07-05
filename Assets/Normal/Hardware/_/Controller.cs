using UnityEngine;
using System.Collections;

namespace Normal.Hardware {
    public delegate void TriggerBeganDelegate(Controller sender);
    public delegate void TriggerActiveDelegate(Controller sender);
    public delegate void TriggerEndedDelegate(Controller sender);

    public delegate void GripBeganDelegate(Controller sender);
    public delegate void GripActiveDelegate(Controller sender);
    public delegate void GripEndedDelegate(Controller sender);

    // Represents a generic hand in the application. Subclasses implement tracking for various hardware.
    public class Controller : MonoBehaviour {
        public event ActiveUpdatedDelegate activeUpdated;
        public event   PoseUpdatedDelegate   poseUpdated;

        // Value between 0 and 1.
        public float triggerPosition;

        public event TriggerBeganDelegate  triggerBegan;
        public event TriggerActiveDelegate triggerActive;
        public event TriggerEndedDelegate  triggerEnded;

        public event GripBeganDelegate     gripBegan;
        public event GripActiveDelegate    gripActive;
        public event GripEndedDelegate     gripEnded;

        // Active
        protected void DelegateFireActiveUpdated(bool activeSelf) {
            if (activeUpdated != null)
                activeUpdated(this, activeSelf);
        }

        // Pose
        protected void DelegateFirePoseUpdated(Vector3 position, Quaternion rotation) {
            if (poseUpdated != null)
                poseUpdated(this, position, rotation);
        }

        // Trigger
        protected void DelegateFireTriggerBegan() {
            if (triggerBegan != null)
                triggerBegan(this);
        }

        protected void DelegateFireTriggerActive() {
            if (triggerActive != null)
                triggerActive(this);
        }

        protected void DelegateFireTriggerEnded() {
            if (triggerEnded != null)
                triggerEnded(this);
        }

        // Grip
        protected void DelegateFireGripBegan() {
            if (gripBegan != null)
                gripBegan(this);
        }

        protected void DelegateFireGripActive() {
            if (gripActive != null)
                gripActive(this);
        }

        protected void DelegateFireGripEnded() {
            if (gripEnded != null)
                gripEnded(this);
        }

        // Haptic
        public virtual void TriggerHapticPulse() {}
    }
}
