using UnityEngine;
using System;
using System.Collections.Generic;

namespace Normal {
    public class Application : Photon.MonoBehaviour {
        public  static Application  sharedApplication { get { return _sharedApplication; } }
        private static Application _sharedApplication;

        // Hardware Player
        [SerializeField, HideInInspector]
        private GameObject              _hardwarePlayerGameObject;
        public  Hardware.HardwarePlayer  hardwarePlayer { get { return _hardwarePlayer; } }
        [SerializeField, HideInInspector]
        private Hardware.HardwarePlayer _hardwarePlayer;

        // Realtime
        private Realtime.Realtime _realtime;
        public  Realtime.Realtime  realtime { get { return _realtime; } }

#if UNITY_EDITOR
        public Type hardwarePlayerClass { get { return _hardwarePlayer != null ? _hardwarePlayer.GetType() : null; } }
        public UnityEditor.SerializedObject GetSerializedObject() { return new UnityEditor.SerializedObject(this); }

        // Hardware Config
        public void CreateHardwarePlayer(Type hardwarePlayerClass) {
            // Destroy existing hardware player if needed
            DestroyHardwarePlayer();

            if (!hardwarePlayerClass.IsSubclassOf(typeof(Hardware.HardwarePlayer))) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Attempting to configure Hardware using class that doesn't derive from Normal.Hardware.HardwarePlayer. Bailing.", "OK");
                return;
            }

            // Get SerializedProperty objects
            UnityEditor.SerializedObject serializedObject = GetSerializedObject();
            serializedObject.Update();
            UnityEditor.SerializedProperty hardwarePlayerProperty           = serializedObject.FindProperty("_hardwarePlayer");
            UnityEditor.SerializedProperty hardwarePlayerGameObjectProperty = serializedObject.FindProperty("_hardwarePlayerGameObject");

            // Create hardware player game object if it doesn't exist yet.
            if (_hardwarePlayerGameObject == null) {
                _hardwarePlayerGameObject = new GameObject("Player (Hardware)");
                _hardwarePlayerGameObject.transform.SetParent(transform, false);
                hardwarePlayerGameObjectProperty.objectReferenceValue = _hardwarePlayerGameObject;
            }

            // If there's an existing hardware player component, enable it. Otherwise create it.
            _hardwarePlayer = (Hardware.HardwarePlayer)_hardwarePlayerGameObject.GetComponent(hardwarePlayerClass);
            if (_hardwarePlayer != null) {
                _hardwarePlayer.enabled = true;
                _hardwarePlayer.EnableDependencies();
            } else {
                _hardwarePlayer = (Hardware.HardwarePlayer)_hardwarePlayerGameObject.AddComponent(hardwarePlayerClass);
                _hardwarePlayer.CreateDependencies();
            }
            hardwarePlayerProperty.objectReferenceValue = _hardwarePlayer;

            // Apply Changes
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public void DestroyHardwarePlayer() {
            if (_hardwarePlayer == null)
                return;

            // Get SerializedProperty objects
            UnityEditor.SerializedObject serializedObject = GetSerializedObject();
            serializedObject.Update();
            UnityEditor.SerializedProperty hardwarePlayerProperty = serializedObject.FindProperty("_hardwarePlayer");

            if (UnityEditor.EditorUtility.DisplayDialog("Heyo, be careful here!", "Is it cool if I destroy the [Camera Rig] instance to make room for a shiny new one?\r\n\r\nAll of your scripts /should/ be on \"Player (Hardware)\", but rules are made to be broken right?\r\n\r\nIf you put some custom stuff on [Camera Rig], that's cool, I won't judge, but hit \"Leave in Place\" to keep it in the scene, otherwise, hit \"Destroy!\" bb.", "Destroy!", "Leave in Place")) {
                _hardwarePlayer.DestroyDependencies();
                DestroyImmediate(_hardwarePlayer);
            } else {
                _hardwarePlayer.DisableDependencies();
                _hardwarePlayer.enabled = false;
            }
            hardwarePlayerProperty.objectReferenceValue = null;

            // Apply Changes
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
#endif

        void Awake() {
            if (_sharedApplication != null) {
                Debug.Log("Normal: Application already exists! This is a huge bug! Moving forward with new application. Destroying the old one.");
                DestroyImmediate(_sharedApplication);
            }
            _sharedApplication = this;

            _realtime = GetComponent<Realtime.Realtime>();
        }

        void Start() {
            LoadSession();
        }

        void LoadSession() {
            // Register with Normal's server for account auth.
            // In the future, this should also retrieve an ID used to talk to the realtime servers.
        }
    }
}
