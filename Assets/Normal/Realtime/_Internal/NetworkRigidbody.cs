using UnityEngine;
using System.Collections;

namespace Normal.Realtime {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PhotonView))]
    public class NetworkRigidbody : Photon.MonoBehaviour {
        public  float interpolatePosition = 1.0f;
        public  float interpolateRotation = 1.0f;
        private float _sendInterval { get { return 1.0f / PhotonNetwork.sendRateOnSerialize; } }

        private Rigidbody _rigidbody;

        private Vector3    _targetSyncPosition;
        private Vector3    _targetSyncVelocity;
        private Quaternion _targetSyncRotation = Quaternion.identity;
#pragma warning disable 0414
        private Vector3    _targetSyncAngularVelocity;
        private double     _networkPacketTimestamp;
#pragma warning restore 0414


        void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
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
            stream.SendNext(_rigidbody.position);
            stream.SendNext(_rigidbody.velocity);
            stream.SendNext(_rigidbody.rotation);
            stream.SendNext(_rigidbody.angularVelocity);
        }

        void DeserializeWithStream(PhotonStream stream, PhotonMessageInfo info) {
            _targetSyncPosition        =    (Vector3)stream.ReceiveNext();
            _targetSyncVelocity        =    (Vector3)stream.ReceiveNext();
            _targetSyncRotation        = (Quaternion)stream.ReceiveNext();
            _targetSyncAngularVelocity =    (Vector3)stream.ReceiveNext();

            _networkPacketTimestamp = info.timestamp;

            // TODO: Check if position / rotation are past their snap distances. If so, snap instantly.
        }

        void UpdateNetworkPosition() {
            // TODO: Refactor this. Reimplement rotation prediction. Use a proper curve for prediction.

            // Position
            Vector3 newVelocity = (_targetSyncPosition - _rigidbody.position) * interpolatePosition / _sendInterval;
            _rigidbody.velocity = newVelocity;
            _targetSyncPosition += (_targetSyncVelocity * Time.fixedDeltaTime * 0.1f);

            // Rotation
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, _targetSyncRotation, Time.fixedDeltaTime * (interpolateRotation / _sendInterval)));

            //_rigidBody.angularVelocity = CalculateAngularVelocity(_targetSyncRotation, _rigidBody.rotation) * Time.fixedDeltaTime * 2.0f;

            /*
            Quaternion deltaRotation = _targetSyncRotation * Quaternion.Inverse(_rigidBody.rotation);
            float magnitude;
            Vector3 axis;
            deltaRotation.ToAngleAxis(out magnitude, out axis);
            Vector3 newAngularVelocity = (magnitude * axis) * interpolatePosition / _sendInterval;
            _rigidBody.angularVelocity = newAngularVelocity;
            */

            //m_TargetSyncRotation3D *= Quaternion.Euler(m_TargetSyncAngularVelocity3D * Time.fixedDeltaTime);

            // move sync rotation slightly in rotation direction
            //m_TargetSyncRotation3D += (m_TargetSyncAngularVelocity3D * Time.fixedDeltaTime * moveAheadRatio);

            // move sync position slightly in the position of velocity
            //m_TargetSyncPosition += (m_TargetSyncVelocity * Time.fixedDeltaTime * k_MoveAheadRatio);


            // Try prediction once smooth interpolation is finished.
            /*
            // TODO: Refactor this so we do the ping addition/subtraction as soon as we get the packet.
            double ping = PhotonNetwork.GetPing() * 0.001;
            double timeSinceLastPacket = PhotonNetwork.time - _networkPacketTimestamp;
            double deltaTime = timeSinceLastPacket + ping;

            Vector3 extrapolatedPosition = _networkPosition + _networkVelocity * (float)deltaTime;
            _rigidBody.position = extrapolatedPosition;
            _rigidBody.velocity = _networkVelocity;
            */
        }

        Vector3 CalculateAngularVelocity(Quaternion q1, Quaternion q2) {
            Quaternion rotationDelta1 = q1 * Quaternion.Inverse(q2);
            float magnitude1;
            Vector3 axis1;
            rotationDelta1.ToAngleAxis(out magnitude1, out axis1);
            Vector3 angularVelocity1 = (magnitude1 * axis1);
            Quaternion rotationDelta2 = q1 * Quaternion.Inverse(q2);
            float magnitude2;
            Vector3 axis2;
            rotationDelta2.ToAngleAxis(out magnitude2, out axis2);
            Vector3 angularVelocity2 = (magnitude2 * axis2);
            return (magnitude1 < magnitude2) ? angularVelocity1 : angularVelocity2;
        }
    }
}
