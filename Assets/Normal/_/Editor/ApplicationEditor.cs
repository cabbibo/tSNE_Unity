using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace Normal {
    [CustomEditor(typeof(Application))]
    public class ApplicationEditor : Editor {
        private Texture _logo;
        
        public enum HardwareOptions { None, SteamVR, OculusVR, PlaystationVR };
        private Type _steamvrHardwareType;
        private Type _oculusvrHardwareType;
        private Type _playstationvrHardwareType;
        private HardwareOptions _hardwareToConfigure;

        string GetResourcesPath() {
            MonoScript monoScript = MonoScript.FromScriptableObject(this);
            string scriptPath = AssetDatabase.GetAssetPath(monoScript);
            string directoryPath = Path.GetDirectoryName(scriptPath);
            return Path.Combine(directoryPath, "Resources");
        }

        void OnEnable() {
            _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(GetResourcesPath(), "NormalEditorUILogo.png"));

            LoadAvailableHardwareTypes();
            LoadConfiguredHardwareSetting();

            ZeroGameObjectTransform();
            SetupPhotonVoiceSettingsIfNeeded();
        }

        public override void OnInspectorGUI() {
            // Logo
            if (_logo)
                GUI.DrawTexture(GUILayoutUtility.GetRect(Screen.width - 30.0f, _logo.height/2.0f, GUI.skin.box), _logo, ScaleMode.ScaleToFit);

            // Hardware selection
            _hardwareToConfigure = (HardwareOptions)EditorGUILayout.EnumPopup("Hardware", _hardwareToConfigure);

            // Configure button
            if (GUILayout.Button("Configure Hardware")) {
                ConfigureHardware();
            }

            // Divider
            GUILayout.Label("");

            // Default Inspector
            DrawDefaultInspector();
        }

        void LoadAvailableHardwareTypes() {
            _steamvrHardwareType       = Type.GetType("Normal.Hardware.SteamVRHardwarePlayer,Assembly-CSharp");
            _oculusvrHardwareType      = Type.GetType("Normal.Hardware.OculusVRHardwarePlayer,Assembly-CSharp");
            _playstationvrHardwareType = Type.GetType("Normal.Hardware.PlaystationVRHardwarePlayer,Assembly-CSharp");
        }

        void LoadConfiguredHardwareSetting() {
            Application application = (Application)target;
            Type hardwarePlayerClass = application.hardwarePlayerClass;

            // SteamVR
            if (_steamvrHardwareType != null&& hardwarePlayerClass == _steamvrHardwareType)
                _hardwareToConfigure = HardwareOptions.SteamVR;

            // OculusVR
            else if (_oculusvrHardwareType != null && hardwarePlayerClass == _oculusvrHardwareType)
                _hardwareToConfigure = HardwareOptions.OculusVR;

            // PlaystationVR
            else if (_playstationvrHardwareType != null && hardwarePlayerClass == _playstationvrHardwareType)
                _hardwareToConfigure = HardwareOptions.PlaystationVR;

            // None
            else
                _hardwareToConfigure = HardwareOptions.None;
        }

        void ConfigureHardware() {
            Application application = (Application)target;

            switch (_hardwareToConfigure) {
                case HardwareOptions.None:
                    // Configure player settings
                    PlayerSettings.virtualRealitySupported = false;

#if UNITY_5_4_OR_NEWER
                    // Remove references to SDK
                    UnityEditorInternal.VR.VREditor.SetVREnabledDevices(BuildTargetGroup.Standalone, new string[] { });
#endif

                    // Configure hardware player
                    application.DestroyHardwarePlayer();
                    break;
                case HardwareOptions.SteamVR:
                    if (_steamvrHardwareType == null) {
                        EditorUtility.DisplayDialog("Error", "Well, this is embarrassing, I can't find SteamVR support. There should be a SteamVRHardwarePlayer class in the project for this to work, but there just isn't. I've looked everywhere :'(", "OK");
                        break;
                    }

                    // Configure player settings
#if UNITY_5_4_OR_NEWER
                    PlayerSettings.virtualRealitySupported = true;

                    // Switch to OpenVR SDK
                    UnityEditorInternal.VR.VREditor.SetVREnabledDevices(BuildTargetGroup.Standalone, new string[] { "OpenVR" });
#else
                    PlayerSettings.virtualRealitySupported = false;
#endif
                    // Configure hardware player
                    application.CreateHardwarePlayer(_steamvrHardwareType);
                    break;
                case HardwareOptions.OculusVR:
                    if (_oculusvrHardwareType == null) {
                        EditorUtility.DisplayDialog("Error", "Well, this is embarrassing, I can't find OculusVR support. There should be an OculusVRHardwarePlayer class in the project, but there just isn't. I've looked everywhere :'(", "OK");
                        break;
                    }

                    // Configure player settings
                    EditorPrefs.SetBool("ignore.Virtual Reality Support", true); // Stop SteamVR from trying to switch this back.
                    PlayerSettings.virtualRealitySupported = true;

#if UNITY_5_4_OR_NEWER
                    // Switch to OpenVR SDK
                    UnityEditorInternal.VR.VREditor.SetVREnabledDevices(BuildTargetGroup.Standalone, new string[] { "Oculus" });
#endif

                    // Configure hardware player
                    application.CreateHardwarePlayer(_oculusvrHardwareType);
                    break;
                case HardwareOptions.PlaystationVR:
                    if (_playstationvrHardwareType == null) {
                        EditorUtility.DisplayDialog("Error", "Well, this is embarrassing, I can't find PlaystationVR support. There should be a PlaystationVRHardwarePlayer class in the project for this to work, but there just isn't. I've looked everywhere :'(", "OK");
                        break;
                    }

                    // Configure player settings
                    //PlayerSettings.virtualRealitySupported = false;

                    // Configure hardware player
                    application.CreateHardwarePlayer(_playstationvrHardwareType);
                    break;
            }
        }

        void ZeroGameObjectTransform() {
            Application application = (Application)target;
            Transform transform = application.gameObject.transform;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        void SetupPhotonVoiceSettingsIfNeeded() {
            Application application = (Application)target;

            PhotonVoiceSettings settings = application.gameObject.AddComponentIfNeeded<PhotonVoiceSettings>();
            if (settings != null) {
                settings.AutoConnect    = true;
                settings.AutoDisconnect = true;
                settings.AutoTransmit   = true;
                settings.VoiceDetection = true;
                settings.VoiceDetectionThreshold = 0.005f;
                settings.PlayDelayMs = 150;
                settings.DebugInfo = false;
                settings.hideFlags = HideFlags.HideInInspector;
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(settings, false);
            }
        }
    }
}
