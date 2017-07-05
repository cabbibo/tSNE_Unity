using UnityEngine;
using System.Collections;

namespace Normal.Hardware {
    // Represents a generic hand in the application. Subclasses implement tracking for various hardware.
    public class Headset : MonoBehaviour {
        public event ActiveUpdatedDelegate activeUpdated;
        public event   PoseUpdatedDelegate   poseUpdated;
        public Camera eyeCamera { get { return _eyeCamera; } }

        protected Camera _eyeCamera;

        protected void DelegateFireActiveUpdated(bool activeSelf) {
            if (activeUpdated != null)
                activeUpdated(this, activeSelf);
        }

        protected void DelegateFirePoseUpdated(Vector3 position, Quaternion rotation) {
            if (poseUpdated != null)
                poseUpdated(this, position, rotation);
        }
    }
}
