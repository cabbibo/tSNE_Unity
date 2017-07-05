using UnityEngine;
using System.Collections;

namespace Normal.Realtime {
    [ExecutionOrder(-90)] // Force Update() to run just after the HardwarePlayer which has an execution script order of -100 or -31000 depending on the device. More importantly, we want to run before the API Player does and we don't have control over scripts added by 3rd-party devs.
    public class Player_Internal : Photon.MonoBehaviour {
        private Application _application;
        private Realtime    _realtime;
        /// Abstraction
        /// <summary>
        /// These are the game objects that avatars and tools should be attached to. They represent a player who is local or remote.
        /// </summary>
        public  Head_Internal      head;
        public  Hand_Internal  leftHand;
        public  Hand_Internal rightHand;

        private GameObject _headGameObject;
        private GameObject _leftHandGameObject;
        private GameObject _rightHandGameObject;

        [HideInInspector] public NetworkController  leftHandNetworkController;
        [HideInInspector] public NetworkController rightHandNetworkController;

        /// Hardware
        /// <summary>
        /// This represents the hardware controllers for local players.
        /// </summary>
        private Hardware.HardwarePlayer _hardwarePlayer;

        /// API Avatar
        /// <summary>
        /// This is the class that runs the game object for the avatar. This can be swapped in/out as spaces are loaded/unloaded.
        /// </summary>
        public  Player  apiPlayer { get { return _apiPlayer; } }
        private Player _apiPlayer;

        void Awake() {
            // Hide this game object from the editor hierarchy as it's internal to how Normal operates.
            //gameObject.hideFlags = HideFlags.HideInHierarchy;

                 _headGameObject =      head.gameObject;
             _leftHandGameObject =  leftHand.gameObject;
            _rightHandGameObject = rightHand.gameObject;

             leftHandNetworkController =  leftHand.GetComponent<NetworkController>();
            rightHandNetworkController = rightHand.GetComponent<NetworkController>();
        }

        void Start() {
            // TODO: Strip this once photon is replaced. It only exists right now because Photon is the one who instantiates this class, not something we control.
            _application = Application.sharedApplication;
            _realtime = _application.realtime;

            _realtime.NetworkPlayerInstantiated(this);
            _realtime.NetworkPlayerRequestAPIPlayer(this);
            _realtime.NetworkPlayerInstantiatedAPIPlayer(this);
        }

        void OnDestroy() {
            _realtime.NetworkPlayerOnDestroy(this);
            _realtime.NetworkPlayerOnDestroyAPIPlayer(this);
            Destroy(_apiPlayer);
            _apiPlayer = null;

            StopTrackingHardwarePlayer();
        }

        public void StartTrackingHardwarePlayer(Hardware.HardwarePlayer hardwarePlayer) {
            if (!photonView.isMine) {
                Debug.Log("Attempting to locally track player that isn't owned by this client. Bailing. This is a bug.");
                return;
            }
            if (_hardwarePlayer != null) {
                Debug.Log("StartTrackingHardwarePlayer called on player that is already tracking locally. Bailing. This is a bug.");
                return;
            }

            _hardwarePlayer = hardwarePlayer;

            _hardwarePlayer.transformUpdated              += new Hardware.TransformUpdatedDelegate(SpaceTransformUpdated);
            _hardwarePlayer.headset.activeUpdated         += new Hardware.ActiveUpdatedDelegate(HeadsetActiveUpdated);
            _hardwarePlayer.leftController.activeUpdated  += new Hardware.ActiveUpdatedDelegate(LeftControllerActiveUpdated);
            _hardwarePlayer.rightController.activeUpdated += new Hardware.ActiveUpdatedDelegate(RightControllerActiveUpdated);
            SyncInitialActiveState();

                 head.StartTrackingHardwarePlayer(_hardwarePlayer.headset);
             leftHand.StartTrackingHardwarePlayer(_hardwarePlayer.leftController);
            rightHand.StartTrackingHardwarePlayer(_hardwarePlayer.rightController);
        }

        public void StopTrackingHardwarePlayer() {
            if (_hardwarePlayer == null)
                return;

            _hardwarePlayer.transformUpdated              -= new Hardware.TransformUpdatedDelegate(SpaceTransformUpdated);
            _hardwarePlayer.headset.activeUpdated         -= new Hardware.ActiveUpdatedDelegate(HeadsetActiveUpdated);
            _hardwarePlayer.leftController.activeUpdated  -= new Hardware.ActiveUpdatedDelegate(LeftControllerActiveUpdated);
            _hardwarePlayer.rightController.activeUpdated -= new Hardware.ActiveUpdatedDelegate(RightControllerActiveUpdated);
            _hardwarePlayer = null;
        }

        void SyncInitialActiveState() {
                 _headGameObject.SetActive(_hardwarePlayer.headset.gameObject.activeSelf);
             _leftHandGameObject.SetActive(_hardwarePlayer.leftController.gameObject.activeSelf);
            _rightHandGameObject.SetActive(_hardwarePlayer.rightController.gameObject.activeSelf);
        }

        void HeadsetActiveUpdated(MonoBehaviour sender, bool activeSelf) {
            _headGameObject.SetActive(activeSelf);
            SyncAPIPlayerActive();
        }

        void LeftControllerActiveUpdated(MonoBehaviour sender, bool activeSelf) {
            _leftHandGameObject.SetActive(activeSelf);
            SyncAPIPlayerActive();
        }

        void RightControllerActiveUpdated(MonoBehaviour sender, bool activeSelf) {
            _rightHandGameObject.SetActive(activeSelf);
            SyncAPIPlayerActive();
        }

        void SpaceTransformUpdated(MonoBehaviour sender, Vector3 localPosition, Quaternion localRotation, Vector3 localScale) {
            // TODO: There are some issues in Unity with updating the parent transform of objects that contain rigidbodies.
            // I've hit up the Unity folks about this and am waiting to hear back before I implement play area position syncing.
            // Ideally, I'd like the play area to be the parent of the player's head and hands. I think this will make moving the play area
            // behave more realistically over the network. However, we might have to ditch using rigidbodies for the velocity calculation stuff.
            // Honestly, I think this will be fine. If the end-developer needs rigidbodies for the player, they can add one and use MovePosition()
            // to move the rigidbody based on the position of the object they're trying to track.
        }

        void FixedUpdate() {
            SyncAPIPlayerActive();
            SyncAPIPlayerPoses();
        }

        void Update() {
            // TODO: Why is this still needed?
            if (photonView.isMine)
                head.GetComponent<PhotonVoiceRecorder>().Transmit = true;

            // Sync trigger position
            SyncHardwareControllerState();

            // TODO: Make sure our Update() method runs before the API.Player's update method to ensure they have the latest values when they're doing their thing.
            // TODO: This is also called in Update (vs just in FixedUpdate) because Unity's rigidbody physics run after FixedUpdate(). If we ditch Rigidbodies for prediction, move entirely to FixedUpdate()
            //       This will allow the end-developer to have the latest prediction data for when their FixedUpdate() is called, which they can then use for physics.
            SyncAPIPlayer();
        }

        // Network Syncing
        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting)
                SerializeWithStream(stream, info);
            else
                DeserializeWithStream(stream, info);
        }

        void SerializeWithStream(PhotonStream stream, PhotonMessageInfo info) {
            stream.SendNext     (_headGameObject.activeSelf);
            stream.SendNext( _leftHandGameObject.activeSelf);
            stream.SendNext(_rightHandGameObject.activeSelf);
        }

        void DeserializeWithStream(PhotonStream stream, PhotonMessageInfo info) {
                 _headGameObject.SetActive((bool)stream.ReceiveNext());
             _leftHandGameObject.SetActive((bool)stream.ReceiveNext());
            _rightHandGameObject.SetActive((bool)stream.ReceiveNext());
            SyncAPIPlayerActive();
        }

        // Hardware Controller State
        void SyncHardwareControllerState() {
            // Only sync controller state for local player.
            if (!photonView.isMine)
                return;

            // TODO: Fold this functionality into _Hand. It doesn't make sense here.
            // TODO: Make sure this line of code runs after Hardware.Controller.Update() which should be running after SteamVR_Controller.Update().
             leftHandNetworkController.SetControllerState(_hardwarePlayer.leftController.triggerPosition);
            rightHandNetworkController.SetControllerState(_hardwarePlayer.rightController.triggerPosition);
        }

        // API
        public void ReplaceAPIPlayerWithPrefab(GameObject prefab) {
            if (_apiPlayer != null) {
                Destroy(_apiPlayer);
                _apiPlayer = null;
            }
            if (prefab != null) {
                GameObject apiPlayerGameObject = Instantiate(prefab);
                _apiPlayer = apiPlayerGameObject.GetComponent<Player>();
                SyncAPIPlayerHardwarePlayer();
                SyncAPIPlayer();
            }
        }

        void SyncAPIPlayer() {
            SyncAPIPlayerActive();
            SyncAPIPlayerPoses();
            SyncAPIPlayerControllerState();
            SyncAPIPlayerVoiceVolume();
        }

        void SyncAPIPlayerHardwarePlayer() {
            _apiPlayer.hardwarePlayer = _hardwarePlayer;
        }

        void SyncAPIPlayerActive() {
            if (_apiPlayer == null)
                return;

            GameObject apiHeadGameObject      = _apiPlayer.head;
            GameObject apiLeftHandGameObject  = _apiPlayer.leftHand;
            GameObject apiRightHandGameObject = _apiPlayer.rightHand;

            if (apiHeadGameObject != null)
                EnableDisableGameObjectUsingGameObject(apiHeadGameObject, _headGameObject);
            if (apiLeftHandGameObject != null)
                EnableDisableGameObjectUsingGameObject(apiLeftHandGameObject, _leftHandGameObject);
            if (apiRightHandGameObject != null)
                EnableDisableGameObjectUsingGameObject(apiRightHandGameObject, _rightHandGameObject);
        }

        void EnableDisableGameObjectUsingGameObject(GameObject gameObject, GameObject observedGameObject) {
            if (gameObject.activeSelf && !observedGameObject.activeSelf)
                gameObject.SetActive(false);
            else if (!gameObject.activeSelf && observedGameObject.activeSelf)
                gameObject.SetActive(true);
        }

        void SyncAPIPlayerPoses() {
            if (_apiPlayer == null)
                return;

            GameObject apiHeadGameObject      = _apiPlayer.head;
            GameObject apiLeftHandGameObject  = _apiPlayer.leftHand;
            GameObject apiRightHandGameObject = _apiPlayer.rightHand;

            // TODO: At some point I want to ditch using rigidbodies for our prediction system. Once that happens, we should also support scale here.
            if (apiHeadGameObject != null) {
                apiHeadGameObject.transform.position      =      head.rigidbody.position;
                apiHeadGameObject.transform.rotation      =      head.rigidbody.rotation;
            }
            if (apiLeftHandGameObject != null) {
                apiLeftHandGameObject.transform.position  =  leftHand.rigidbody.position;
                apiLeftHandGameObject.transform.rotation  =  leftHand.rigidbody.rotation;
            }
            if (apiRightHandGameObject != null) {
                apiRightHandGameObject.transform.position = rightHand.rigidbody.position;
                apiRightHandGameObject.transform.rotation = rightHand.rigidbody.rotation;
            }
        }

        void SyncAPIPlayerControllerState() {
            if (_apiPlayer == null)
                return;

            GameObject apiLeftHandGameObject  = _apiPlayer.leftHand;
            GameObject apiRightHandGameObject = _apiPlayer.rightHand;

            if (apiLeftHandGameObject != null) {
                Hand apiHand = apiLeftHandGameObject.GetComponent<Hand>();
                if (apiHand != null)
                    apiHand.triggerPosition = leftHandNetworkController.triggerPosition;
            }
            if (apiRightHandGameObject != null) {
                Hand apiHand = apiRightHandGameObject.GetComponent<Hand>();
                if (apiHand != null)
                    apiHand.triggerPosition = rightHandNetworkController.triggerPosition;
            }
        }

        void SyncAPIPlayerVoiceVolume() {
            if (_apiPlayer == null)
                return;

            GameObject apiHeadGameObject = _apiPlayer.head;

            Head apiHead = apiHeadGameObject.GetComponent<Head>();
            apiHead.voiceVolume = head.voiceVolumeLevel;
        }
    }
}
