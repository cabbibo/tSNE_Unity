using UnityEngine;
using System;
using ExitGames.Client.Photon.Voice;

namespace Normal.Realtime {
    public class Head_Internal : Photon.MonoBehaviour {
        public  new Rigidbody  rigidbody { get { return _rigidbody; } }
        private     Rigidbody _rigidbody;

        private Hardware.Headset _hardwareHeadset;

        private float[] _voiceLocalBuffer;
        private AudioSource _voiceAudioSource;

        public float voiceVolumeLevel { get { return CalculateVoiceVolume(); } }

        void Awake() {
            _rigidbody        = GetComponent<Rigidbody>();
            _voiceAudioSource = GetComponent<AudioSource>();
        }

        void OnDestroy() {
            StopTrackingHardwarePlayer();
        }

        public void StartTrackingHardwarePlayer(Hardware.Headset hardwareHeadset) {
            if (!photonView.isMine) {
                Debug.Log("Attempting to locally track player that isn't owned by this client. Bailing. This is a bug.");
                return;
            }
            if (_hardwareHeadset != null) {
                Debug.Log("StartTrackingHardwarePlayer called on head that is already tracking locally. Bailing. This is a bug.");
                return;
            }

            _hardwareHeadset = hardwareHeadset;

            _hardwareHeadset.poseUpdated += new Hardware.PoseUpdatedDelegate(PoseUpdated);
            SyncInitialPose();
        }

        public void StopTrackingHardwarePlayer() {
            if (_hardwareHeadset == null)
                return;

            _hardwareHeadset.poseUpdated -= new Hardware.PoseUpdatedDelegate(PoseUpdated);
            _hardwareHeadset = null;
        }

        void SyncInitialPose() {
            _rigidbody.position = _hardwareHeadset.transform.position;
            _rigidbody.rotation = _hardwareHeadset.transform.rotation;
        }

        void PoseUpdated(MonoBehaviour sender, Vector3 position, Quaternion rotation) {
            _rigidbody.MovePosition(position);
            _rigidbody.MoveRotation(rotation);
        }

        float[] GetLocalVoiceBuffer() {
            PhotonVoiceRecorder pvn = GetComponent<PhotonVoiceRecorder>();
            System.Reflection.FieldInfo voiceFieldInfo = typeof(PhotonVoiceRecorder).GetField("voice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            LocalVoice localVoice = (LocalVoice)voiceFieldInfo.GetValue(pvn);
            System.Reflection.FieldInfo bufferFieldInfo = typeof(LocalVoice).GetField("sourceFrameBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (float[])bufferFieldInfo.GetValue(localVoice);
        }

        float CalculateVoiceVolume() {
            float averageDbSample = -42.0f;
            
            if (photonView.isMine) {
                if (_voiceLocalBuffer == null)
                    _voiceLocalBuffer = GetLocalVoiceBuffer();

                if (_voiceLocalBuffer != null) {
                    float[] samples = new float[256];
                    Array.Copy(_voiceLocalBuffer, _voiceLocalBuffer.Length-256, samples, 0, 256);

                    averageDbSample = Utility.CalculateAverageDbForAudioBuffer(samples);
                }
            } else {
                if (_voiceAudioSource.isPlaying) {
                    float[] samples = new float[256];
                    _voiceAudioSource.GetOutputData(samples, 0);

                    averageDbSample = Utility.CalculateAverageDbForAudioBuffer(samples);
                }
            }

            // These are arbitrary values I picked from my own testing.
            float volumeMinDb = -42.0f;
            float volumeMaxDb = -10.0f;
            float volumeRange = volumeMaxDb - volumeMinDb;

            float normalizedVolume = (averageDbSample - volumeMinDb) / volumeRange;
            if (normalizedVolume < 0.0f)
                normalizedVolume = 0.0f;
            if (normalizedVolume > 1.0f)
                normalizedVolume = 1.0f;

            return normalizedVolume;
        }
    }
}
