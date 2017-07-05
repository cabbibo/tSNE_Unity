using UnityEngine;
using Valve.VR;
using System.Collections;

namespace Normal.Hardware {
    [ExecutionOrder(-31000)] // Force Update() to run just after SteamVR_Render.Update() which has an execution script order of -32000.
    public class SteamVRHeadset : Headset {
        private SteamVR_TrackedObject _trackedObject;

#if UNITY_5_4_OR_NEWER
        public void SetCamera(Camera eyeCamera) {
            _eyeCamera = eyeCamera;
        }

        void Update() {
            transform.localPosition = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head);
            transform.localRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head);

            DelegateFirePoseUpdated(transform.position, transform.rotation);
        }
#else
        public void TrackHead(SteamVR_TrackedObject trackedObject, Camera eyeCamera) {
            _trackedObject = trackedObject;
            _eyeCamera     = eyeCamera;
        }

        void OnEnable() {
            SteamVR_Utils.Event.Listen("new_poses", OnNewPoses);
            DelegateFireActiveUpdated(gameObject.activeSelf);
        }

        void OnDisable() {
            SteamVR_Utils.Event.Remove("new_poses", OnNewPoses);
            DelegateFireActiveUpdated(gameObject.activeSelf);
        }

        void OnNewPoses(params object[] args) {
            if (_trackedObject.index == SteamVR_TrackedObject.EIndex.None)
                return;

            int i = (int)_trackedObject.index;

            TrackedDevicePose_t[] poses = (TrackedDevicePose_t[])args[0];
            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            SteamVR_Utils.RigidTransform pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            Vector3    worldPosition = _trackedObject.transform.parent.TransformPoint(pose.pos);
            Quaternion worldRotation = _trackedObject.transform.parent.rotation * pose.rot;
            PositionGeometry(worldPosition, worldRotation);
            DelegateFirePoseUpdated(worldPosition, worldRotation);
        }

        void PositionGeometry(Vector3 position, Quaternion rotation) {
            transform.position = position;
            transform.rotation = rotation;
        }
#endif
    }
}
