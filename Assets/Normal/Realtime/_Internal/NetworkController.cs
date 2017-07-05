using UnityEngine;
using System.Collections;

namespace Normal.Realtime {
    [RequireComponent(typeof(PhotonView))]
    public class NetworkController : Photon.MonoBehaviour {
        public  float interpolatePosition = 1.0f;
        public  float interpolateRotation = 1.0f;
        private float _sendInterval { get { return 1.0f / PhotonNetwork.sendRateOnSerialize; } }

        public  float  triggerPosition { get { return _triggerPosition; } }
        private float _triggerPosition;
        private float _targetTriggerPosition;

        public void SetControllerState(float triggerPosition) {
            _triggerPosition = triggerPosition;
        }

        void FixedUpdate() {
            if (!photonView.isMine)
                UpdateNetworkPosition();
        }

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting)
                SerializeWithStream(stream, info);
            else
                DeserializeWithStream(stream, info);
        }

        void SerializeWithStream(PhotonStream stream, PhotonMessageInfo info) {
            stream.SendNext(_triggerPosition);
        }

        void DeserializeWithStream(PhotonStream stream, PhotonMessageInfo info) {
            _targetTriggerPosition = (float)stream.ReceiveNext();

            //_networkPacketTimestamp = info.timestamp;

            // TODO: Check if position / rotation are past their snap distances. If so, snap instantly.
        }

        void UpdateNetworkPosition() {
            // TODO: Refactor this. Use a proper curve for prediction.
            _triggerPosition = Mathf.Lerp(_triggerPosition, _targetTriggerPosition, Time.fixedDeltaTime * (interpolateRotation / _sendInterval));
        }
    }
}
