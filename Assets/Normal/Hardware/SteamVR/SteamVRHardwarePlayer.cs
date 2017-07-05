using UnityEngine;
using System.Collections;

namespace Normal.Hardware {
    [DisallowMultipleComponent]
    public class SteamVRHardwarePlayer : HardwarePlayer {
        [Header("Steam VR")]
        public  GameObject                     cameraRigGameObject;
        public  SteamVR_TrackedObject         headsetTrackedObject;
        public  Camera                                   eyeCamera;
        public  SteamVR_TrackedObject  leftControllerTrackedObject;
        public  SteamVR_TrackedObject rightControllerTrackedObject;

#if UNITY_EDITOR
        // Used by the editor to create / destroy dependencies in the scene.
        public override void CreateDependencies() {
            // Create HardwarePlayer headset / controllers
            CreateHardwarePlayer();

            // Create SteamVR [Camera Rig] prefab and wire it up.
            CreateCameraRig();
        }

        public override void DestroyDependencies() {
            DestroyHardwarePlayer();
            DestroyCameraRig();
        }

        // Used by the editor to enable / disable dependencies in the scene.
        public override void EnableDependencies() {
            cameraRigGameObject.SetActive(true);
        }

        public override void DisableDependencies() {
            cameraRigGameObject.SetActive(false);
        }


        //// Hardware Player
        void CreateHardwarePlayer() {
            CreateHeadsetAndControllersIfNeeded();

            _headset         = _headsetGameObject.AddComponent<SteamVRHeadset>();
            _leftController  = _leftControllerGameObject.AddComponent<SteamVRController>();
            _rightController = _rightControllerGameObject.AddComponent<SteamVRController>();
        }

        void DestroyHardwarePlayer() {
            // We don't want to destroy the headset / controller game objects incase other scripts have been added to the hardware player. Instead we just destroy our scripts on them.
            DestroyImmediate(_headset);
            DestroyImmediate(_leftController);
            DestroyImmediate(_rightController);
            _headset         = null;
            _leftController  = null;
            _rightController = null;
        }

        //// SteamVR Camera Rig
        void CreateCameraRig() {
            // Set up SteamVR's CameraRig and grab references to the necessary components.
            string[] guids = UnityEditor.AssetDatabase.FindAssets("[Camera Rig]");
            if (guids.Length == 0) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to find SteamVR's [Camera Rig] prefab. Please make sure it exists in the project and has not been renamed.", "OK");
                return;
            } else if (guids.Length > 1) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Multiple prefabs with the name [Camera Rig] were found. Something is wrong. Bailing.", "OK");
                return;
            }

            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject cameraRigPrefab = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
            cameraRigGameObject = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(cameraRigPrefab);
            if (cameraRigGameObject == null) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to instantiate [Camera Rig] prefab. Something is wrong. Bailing.", "OK");
                return;
            }
            // Set the [Camera Rig]'s parent to the same as the HardwarePlayer (At the time of writing, this is Normal's Application game object)
            cameraRigGameObject.transform.SetParent(transform.parent, false);

            // Headset
            Transform headsetTransform = cameraRigGameObject.transform.Find("Camera (head)");
            if (headsetTransform == null) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to find \"Camera (head)\" in [Camera Rig] prefab. Something is wrong. Bailing.", "OK");
                return;
            }
            headsetTrackedObject = headsetTransform.GetComponent<SteamVR_TrackedObject>();
            if (headsetTrackedObject == null) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to find SteamVR_TrackedObject component on \"Camera (head)\" in [Camera Rig] prefab. Something is wrong. Bailing.", "OK");
                return;
            }

            // Eye Camera
            Transform cameraTransform = headsetTransform.Find("Camera (eye)");
            if (cameraTransform == null) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to find \"Camera (eye)\" in \"Camera (head)\" in [Camera Rig] prefab. Something is wrong. Bailing.", "OK");
                return;
            }
            eyeCamera = cameraTransform.GetComponent<Camera>();
            if (eyeCamera == null) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to find Camera component on \"Camera (eye)\" in \"Camera (head)\" in [Camera Rig] prefab. Something is wrong. Bailing.", "OK");
                return;
            }

            // Left Controller
            Transform leftControllerTransform = cameraRigGameObject.transform.Find("Controller (left)");
            if (leftControllerTransform == null) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to find \"Controller (left)\" in [Camera Rig] prefab. Something is wrong. Bailing.", "OK");
                return;
            }
            leftControllerTrackedObject = leftControllerTransform.GetComponent<SteamVR_TrackedObject>();
            if (leftControllerTrackedObject == null) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to find SteamVR_TrackedObject component on \"Controller (left)\" in [Camera Rig] prefab. Something is wrong. Bailing.", "OK");
                return;
            }

            // Right Controller
            Transform rightControllerTransform = cameraRigGameObject.transform.Find("Controller (right)");
            if (rightControllerTransform == null) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to find \"Controller (right)\" in [Camera Rig] prefab. Something is wrong. Bailing.", "OK");
                return;
            }
            rightControllerTrackedObject = rightControllerTransform.GetComponent<SteamVR_TrackedObject>();
            if (rightControllerTrackedObject == null) {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Unable to find SteamVR_TrackedObject component on \"Controller (right)\" in [Camera Rig] prefab. Something is wrong. Bailing.", "OK");
                return;
            }
        }

        void DestroyCameraRig() {
                    headsetTrackedObject = null;
                               eyeCamera = null;
             leftControllerTrackedObject = null;
            rightControllerTrackedObject = null;

            DestroyImmediate(cameraRigGameObject);
            cameraRigGameObject = null;
        }
#endif

        void Awake() {
            TrackHeadset((SteamVRHeadset)_headset, headsetTrackedObject, eyeCamera);
            TrackController((SteamVRController)_leftController,  leftControllerTrackedObject);
            TrackController((SteamVRController)_rightController, rightControllerTrackedObject);

            _headset.poseUpdated         += new PoseUpdatedDelegate(PoseUpdated);
            _leftController.poseUpdated  += new PoseUpdatedDelegate(PoseUpdated);
            _rightController.poseUpdated += new PoseUpdatedDelegate(PoseUpdated);
        }

        void OnDestroy() {
            _headset.poseUpdated         -= new PoseUpdatedDelegate(PoseUpdated);
            _leftController.poseUpdated  -= new PoseUpdatedDelegate(PoseUpdated);
            _rightController.poseUpdated -= new PoseUpdatedDelegate(PoseUpdated);
        }

        void TrackHeadset(SteamVRHeadset headset, SteamVR_TrackedObject trackedObject, Camera eyeCamera) {
#if UNITY_5_4_OR_NEWER
            headset.SetCamera(eyeCamera);
#else
            headset.TrackHead(trackedObject, eyeCamera);
#endif
        }

        void TrackController(SteamVRController controller, SteamVR_TrackedObject trackedObject) {
            controller.TrackObject(trackedObject);
        }

        void Update() {
            EnableDisableHeadAndHands();
            SyncCameraRigTransform();
        }

        // State Management
        void EnableDisableHeadAndHands() {
#if UNITY_5_4_OR_NEWER
            EnableDisableGameObjectIfNeeded(_headset.gameObject, UnityEngine.VR.VRDevice.isPresent);
#else
            EnableDisableGameObjectUsingTrackedObject(_headset.gameObject, headsetTrackedObject);
#endif
            EnableDisableGameObjectUsingTrackedObject(_leftController.gameObject, leftControllerTrackedObject);
            EnableDisableGameObjectUsingTrackedObject(_rightController.gameObject, rightControllerTrackedObject);
        }

        void EnableDisableGameObjectIfNeeded(GameObject gameObject, bool enabled) {
            if (gameObject.activeSelf && !enabled)
                gameObject.SetActive(false);
            else if (!gameObject.activeSelf && enabled)
                gameObject.SetActive(true);
        }

        void EnableDisableGameObjectUsingTrackedObject(GameObject gameObject, SteamVR_TrackedObject trackedObject) {
            if (gameObject.activeSelf && !trackedObject.isValid)
                gameObject.SetActive(false);
            else if (!gameObject.activeSelf && trackedObject.isValid)
                gameObject.SetActive(true);
        }

        // Transform Syncing
        void PoseUpdated(MonoBehaviour sender, Vector3 localPosition, Quaternion localRotation) {
            SyncCameraRigTransform();
        }

        void SyncCameraRigTransform() {
            if (cameraRigGameObject == null) {
                Debug.Log("Failed to sync camera rig. No camera rig set for SteamVR hardware player.");
                return;
            }

            Transform cameraRigTransform = cameraRigGameObject.transform;
            bool transformChanged = cameraRigTransform.localPosition != transform.localPosition ||
                                    cameraRigTransform.localRotation != transform.localRotation ||
                                    cameraRigTransform.localScale    != transform.localScale;

            transform.localPosition = cameraRigGameObject.transform.localPosition;
            transform.localRotation = cameraRigGameObject.transform.localRotation;
            transform.localScale    = cameraRigGameObject.transform.localScale;

            if (transformChanged)
                DelegateFireTransformUpdated(transform.localPosition, transform.localRotation, transform.localScale);
        }
    }
}
