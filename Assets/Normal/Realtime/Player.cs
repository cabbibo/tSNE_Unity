using UnityEngine;
using System.Collections;

namespace Normal.Realtime {
    public class Player : MonoBehaviour {
        /// <summary>
        /// A reference to the hardware player. This is only populated for local players. Use this class to get hardware events such as button and trigger presses.
        /// </summary>
        [HideInInspector]
        public Hardware.HardwarePlayer hardwarePlayer;

        public GameObject head;
        public GameObject leftHand;
        public GameObject rightHand;

        void Awake() {
            // Create Head and Hands if they don't exist yet.
            CreateHeadAndHands();
        }

        void CreateHeadAndHands() {
            if (     head == null)      head = CreateChildGameObject("Head");
            if ( leftHand == null)  leftHand = CreateChildGameObject("Left Hand");
            if (rightHand == null) rightHand = CreateChildGameObject("Right Hand");

                 head.AddComponentIfNeeded<Head>();
             leftHand.AddComponentIfNeeded<Hand>();
            rightHand.AddComponentIfNeeded<Hand>();
        }

        GameObject CreateChildGameObject(string name) {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(transform, false);
            return gameObject;
        }

        void AddComponentIfNeeded<T>(GameObject gameObject) where T : Component {
            T component = gameObject.GetComponent<T>();
            if (component == null)
                gameObject.AddComponent<T>();
        }
    }
}
