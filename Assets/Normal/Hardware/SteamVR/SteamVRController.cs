using UnityEngine;
using Valve.VR;
using System.Collections;

namespace Normal.Hardware {
    [ExecutionOrder(-31000)] // Force Update() to run just after SteamVR_Render.Update() which has an execution script order of -32000.
    public class SteamVRController : Controller {
        public  SteamVR_TrackedObject  trackedObject { get { return _trackedObject; } }
        private SteamVR_TrackedObject _trackedObject;

        public void TrackObject(SteamVR_TrackedObject trackedObject) {
            _trackedObject = trackedObject;
        }

        void OnEnable() {
            SteamVR_Events.NewPoses.Listen(OnNewPoses);
            DelegateFireActiveUpdated(gameObject.activeSelf);
        }

        void OnDisable() {
            SteamVR_Events.NewPoses.Remove(OnNewPoses);
            DelegateFireActiveUpdated(gameObject.activeSelf);
        }

       void OnNewPoses(TrackedDevicePose_t[] poses) {
            if (_trackedObject.index == SteamVR_TrackedObject.EIndex.None)
                return;

            int i = (int)_trackedObject.index;

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

        void Update() {
            if (_trackedObject.index == SteamVR_TrackedObject.EIndex.None) {
                // No device
                // TODO: Cancel events that were in progress?
                return;
            }

            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)_trackedObject.index);
            triggerPosition = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger).x;

            // Trigger
            bool hairTriggerBegan  = device.GetHairTriggerDown();
            bool hairTriggerActive = device.GetHairTrigger();
            bool hairTriggerEnded  = device.GetHairTriggerUp();

            if (hairTriggerBegan)  DelegateFireTriggerBegan();
            if (hairTriggerActive) DelegateFireTriggerActive();
            if (hairTriggerEnded)  DelegateFireTriggerEnded();

            // Grip
            bool gripBegan  = device.GetPressDown(EVRButtonId.k_EButton_Grip);
            bool gripActive = device.GetPress(EVRButtonId.k_EButton_Grip);
            bool gripEnded  = device.GetPressUp(EVRButtonId.k_EButton_Grip);

            if (gripBegan)  DelegateFireGripBegan();
            if (gripActive) DelegateFireGripActive();
            if (gripEnded)  DelegateFireGripEnded();
        }

        private bool _hapticPulseActive = false;

        public override void TriggerHapticPulse() {
            if (_hapticPulseActive)
                return;

            StartCoroutine(HapticPulse());
            _hapticPulseActive = true;
        }

        IEnumerator HapticPulse() {
            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)_trackedObject.index);
            // When I wrote this, two frames of vibration felt like a really solid hit without it feeling sloppy.
            // Also using WaitForEndOfFrame() to match my implementation for the Oculus Touch controllers. Not sure if this is ideal.
            device.TriggerHapticPulse(1500);
            yield return new WaitForEndOfFrame();
            device.TriggerHapticPulse(1500);
            yield return new WaitForEndOfFrame();
            _hapticPulseActive = false;
        }
    }
}
