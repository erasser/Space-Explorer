using System;
using System.Collections.Generic;
using soulercoaster.scripts;
using UnityEngine;

namespace soulercoasterLite.scripts {
    [Serializable]
    public struct PlaneConfiguration {
        public float width;
        public float distance;
        public float angle;
        public float skew;
        public float planeAngle;
    }

    [RequireComponent(typeof(SoulerCoaster))]
    [ExecuteInEditMode]
    public class CustomPlaneConfiguration : MonoBehaviour {
        public List<PlaneConfiguration> planes = new();
        private bool shouldRender = true;
        public bool generateOnChange = true;

        public void OnValidate() {
            if (generateOnChange) {
                shouldRender = true;
            }
        }

        private void Update() {
            if (shouldRender) {
                GetComponent<SoulerCoaster>().generate();
                shouldRender = false;
            }
        }
    }
}