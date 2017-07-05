using UnityEngine;
using System.Collections.Generic;

namespace Normal.Realtime {
    [RequireComponent(typeof(Application))]
    public class Realtime : MonoBehaviour {
        // Application
        [SerializeField, HideInInspector]
        private Application _application;

        public bool joinRoomOnStart = true;
        
        // Players
        [SerializeField]
        private GameObject _playerPrefab;
        public  GameObject  playerPrefab { get { return _playerPrefab; } set { SetPlayerPrefab(value); } }

        private         Player  _localPlayer;
        public          Player   localPlayer { get { return _localPlayer; } }
        private HashSet<Player> _players;
        public  HashSet<Player>  players     { get { return _players;     } }

        // Players Internal
        private string _internalPlayerPrefabName = "Player (Internal)";
        private         Player_Internal  _localInternalPlayer;
        private HashSet<Player_Internal> _internalPlayers;

        void Awake() {
            // Application
            _application = GetComponent<Application>();

            // Networking
            InitializePhoton();

            // Players
            _players         = new HashSet<Player>();
            _internalPlayers = new HashSet<Player_Internal>();
        }

        // Photon
        void InitializePhoton() {
            string gameVersion = UnityEngine.Application.productName + " (" + UnityEngine.Application.version + ")";
            Debug.Log("Connecting to server as: \"" + gameVersion + "\"");

#pragma warning disable 0219
            PhotonVoiceNetwork photonVoiceNetwork = PhotonVoiceNetwork.instance; // Workaround for Photon Voice bug: http://forum.photonengine.com/discussion/7496/photon-voice-debugging
#pragma warning restore 0219
            PhotonNetwork.sendRate = 40;
            PhotonNetwork.sendRateOnSerialize = 20;
            PhotonNetwork.ConnectUsingSettings(gameVersion);
        }

        // GUI
        void OnGUI() {
            if (!PhotonNetwork.connected) {
                GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
            }
        }

        // Photon
        void OnConnectedToMaster() {
            if (joinRoomOnStart)
                PhotonNetwork.JoinOrCreateRoom("default", new RoomOptions { MaxPlayers = 0 }, null);
        }

        void OnJoinedRoom() {
            Debug.Log("Joined room " + PhotonNetwork.room);
            NetworkPlayerCreateLocalPlayer();
        }

        //// Player
        // Internal Player
        void NetworkPlayerCreateLocalPlayer() {
            if (_localInternalPlayer != null) {
                Debug.Log("Attempting to create local network player while one already exists. This is a bug! Bailing.");
                return;
            }

            GameObject playerGameObject = PhotonNetwork.Instantiate(_internalPlayerPrefabName, Vector3.zero, Quaternion.identity, 0);
            _localInternalPlayer = playerGameObject.GetComponent<Player_Internal>();
            _localInternalPlayer.StartTrackingHardwarePlayer(_application.hardwarePlayer);
        }

        public void NetworkPlayerInstantiated(Player_Internal player) {
            _internalPlayers.Add(player);
        }

        public void NetworkPlayerOnDestroy(Player_Internal player)
        {
            try
            {
                player.apiPlayer.gameObject.SetActive(false); /////////////
            } catch
            {

            }

            _internalPlayers.Remove(player);
        }

        public void NetworkPlayerRequestAPIPlayer(Player_Internal networkPlayer) {
            if (_playerPrefab != null)
                networkPlayer.ReplaceAPIPlayerWithPrefab(_playerPrefab);
        }

        public void NetworkPlayerInstantiatedAPIPlayer(Player_Internal player) {
            PlayerJoined(player.apiPlayer);
        }

        public void NetworkPlayerOnDestroyAPIPlayer(Player_Internal player) {
            PlayerLeft(player.apiPlayer);
        }

        // Public Player
        void SetPlayerPrefab(GameObject playerPrefab) {
            _playerPrefab = playerPrefab;

            // Replace all public players
            foreach (Player_Internal player in _internalPlayers) {
                player.ReplaceAPIPlayerWithPrefab(_playerPrefab);
            }

            // Refresh public variables used to access players from this class
            RefreshPlayersLists();
        }

        void PlayerJoined(Player apiPlayer) {
            RefreshPlayersLists();
            OnPlayerJoinedRoom(apiPlayer);
        }

        void PlayerLeft(Player apiPlayer) {
            RefreshPlayersLists();
            OnPlayerLeftRoom(apiPlayer);
        }

        // Called when players leave / join or when the prefab is switched
        void RefreshPlayersLists() {
            // Refresh Local Player
            _localPlayer = _localInternalPlayer.apiPlayer;

            // Refresh Players
            HashSet<Player> players = new HashSet<Player>();
            foreach (Player_Internal internalPlayer in _internalPlayers)
                players.Add(internalPlayer.apiPlayer);
            _players = players;
        }

        // Player Events
        /// <summary>
        /// Called when a player joins the room.
        /// </summary>
        public  delegate void PlayerJoinedRoom(Player player);
        public  event PlayerJoinedRoom playerJoinedRoom;
        private void OnPlayerJoinedRoom(Player player) {
            if (playerJoinedRoom != null)
                playerJoinedRoom(player);
        }

        /// <summary>
        /// Called when a player leaves the room.
        /// </summary>
        public delegate void PlayerLeftRoom(Player player);
        public  event PlayerLeftRoom playerLeftRoom;
        private void OnPlayerLeftRoom(Player player) {
            if (playerLeftRoom != null)
                playerLeftRoom(player);
        }
    }
}
