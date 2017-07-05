using UnityEngine;
using System.Collections;

namespace Normal.Realtime {
    public class VoiceScale : MonoBehaviour {
        private Head _head;

        void Awake() {
            _head = GetComponent<Head>();
        }

        void Update() {
            float scale = 1.5f + _head.voiceVolume*.2f;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
