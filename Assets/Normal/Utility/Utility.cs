using UnityEngine;
using System;

namespace Normal {
    public class Utility {
        public static void SetLayerRecursively(GameObject gameObject, int layer) {
            if (gameObject == null)
                return;

            gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        public static float CalculateAverageDbForAudioBuffer(float[] audioBuffer) {
            float averageDbSample = 0.0f;
            for (int i = 0; i < audioBuffer.Length; i++) {
                float dbSample = audioBuffer[i];
                averageDbSample += dbSample * dbSample;
            }

            averageDbSample = Mathf.Sqrt(averageDbSample / audioBuffer.Length);
            averageDbSample = LinearToDb(averageDbSample);
            //averageDbSample = Mathf.Exp(-2.0f * averageDbSample) * averageDbSample;
            return averageDbSample;
        }

        public static float LinearToDb(float linear) {
            float db = -100.0f;

            if (linear != 0.0f)
                db = 20.0f * Mathf.Log10(linear);

            return db;
        }
    }

    public static class Extensions {
        // Will only add a component if one doesn't exist already. If a new component is created, it will be returned.
        public static T AddComponentIfNeeded<T>(this GameObject gameObject) where T : Component {
            T component = gameObject.GetComponent<T>();
            if (component != null)
                return null;

            return gameObject.AddComponent<T>(); ;
        }
    }

    public class ExecutionOrder : Attribute {
        public int order;

        public ExecutionOrder(int order) {
            this.order = order;
        }
    }
}
