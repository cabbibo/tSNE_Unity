using UnityEngine;
using System.Collections;

namespace Normal.Hardware {
    public delegate void ActiveUpdatedDelegate(MonoBehaviour sender, bool activeSelf);
    public delegate void PoseUpdatedDelegate(MonoBehaviour sender, Vector3 position, Quaternion rotation);
    public delegate void TransformUpdatedDelegate(MonoBehaviour sender, Vector3 localPosition, Quaternion localRotation, Vector3 localScale);

    // Represents a generic play area / space. This also represents the parent transform of the head / hands objects.
    public class HardwarePlayer : MonoBehaviour {
        // Abstraction
        public Headset    headset         { get { return _headset;         } }
        public Controller leftController  { get { return _leftController;  } }
        public Controller rightController { get { return _rightController; } }

        // These are set by the editor script, and we want to serialize those values.
        [SerializeField] protected Headset    _headset;
        [SerializeField] protected Controller _leftController;
        [SerializeField] protected Controller _rightController;

        // These should be populated by calling CreateHeadsetAndControllersIfNeeded() in Awake().
        protected GameObject _headsetGameObject;
        protected GameObject _leftControllerGameObject;
        protected GameObject _rightControllerGameObject;

#if UNITY_EDITOR
        // Used by the editor to create / destroy dependencies in the scene.
        public virtual void  CreateDependencies() { }
        public virtual void DestroyDependencies() { }

        // Used by the editor to enable / disable dependencies in the scene.
        public virtual void  EnableDependencies() { }
        public virtual void DisableDependencies() { }

        protected void CreateHeadsetAndControllersIfNeeded() {
            _headsetGameObject         = CreateGameObjectIfNeeded("Headset");
            _leftControllerGameObject  = CreateGameObjectIfNeeded("Left Controller");
            _rightControllerGameObject = CreateGameObjectIfNeeded("Right Controller");
        }

        GameObject CreateGameObjectIfNeeded(string name) {
            GameObject gameObject = null;

            Transform gameObjectTransform = transform.Find(name);
            if (gameObjectTransform != null)
                gameObject = gameObjectTransform.gameObject;

            if (gameObject == null) {
                gameObject = new GameObject(name);
                gameObject.transform.SetParent(transform, false);
            }

            return gameObject;
        }
#endif

        public event TransformUpdatedDelegate transformUpdated;

        protected void DelegateFireTransformUpdated(Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            if (transformUpdated != null)
                transformUpdated(this, position, rotation, localScale);
        }
    }
}
