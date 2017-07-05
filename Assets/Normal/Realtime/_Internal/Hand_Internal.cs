using UnityEngine;
using System.Collections;

namespace Normal.Realtime {
    public class Hand_Internal : Photon.MonoBehaviour {
        public  new Rigidbody  rigidbody { get { return _rigidbody; } }
        private     Rigidbody _rigidbody;

        private Hardware.Controller _hardwareController;

        void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
        }

        void OnDestroy() {
            StopTrackingHardwarePlayer();
        }

        public void StartTrackingHardwarePlayer(Hardware.Controller hardwareController) {
            if (!photonView.isMine) {
                Debug.Log("Attempting to locally track player that isn't owned by this client. Bailing. This is a bug.");
                return;
            }
            if (_hardwareController != null) {
                Debug.Log("StartTrackingHardwarePlayer called on hand that is already tracking locally. Bailing. This is a bug.");
                return;
            }

            _hardwareController = hardwareController;

            _hardwareController.poseUpdated += new Hardware.PoseUpdatedDelegate(PoseUpdated);
            SyncInitialPose();
        }

        public void StopTrackingHardwarePlayer() {
            if (_hardwareController == null)
                return;

            _hardwareController.poseUpdated -= new Hardware.PoseUpdatedDelegate(PoseUpdated);
            _hardwareController = null;
        }

        void SyncInitialPose() {
            _rigidbody.position = _hardwareController.transform.position;
            _rigidbody.rotation = _hardwareController.transform.rotation;
        }

        void PoseUpdated(MonoBehaviour sender, Vector3 position, Quaternion rotation) {
            _rigidbody.MovePosition(position);
            _rigidbody.MoveRotation(rotation);
        }
    }
}
