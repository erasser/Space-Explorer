using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace soulercoasterLite.scripts.pathGenerators {
    [ExecuteInEditMode]
    [RequireComponent(typeof(LineRenderer))]
    public class Connector : MonoBehaviour {
        [Header("Generates a line between 2 or more transforms")]
        public List<Transform> transforms = new();

        public int resolution = 1;
        public bool liveUpdate = true;

        public void OnValidate() {
            this.setupLineRenderer();
            if (liveUpdate && transforms.Count > 1) {
                generate();
            }
        }


        [ContextMenu("generate()")]
        public void generate() {
            if (transforms.Count < 2) {
                return;
            }

            var points = new List<Vector3>();
            for (var k = 1; k < transforms.Count; k++) {
                for (var i = 0f; i < resolution; i++) {
                    points.Add(transform.InverseTransformPoint(Vector3.Lerp(transforms[k - 1].position,
                        transforms[k].position, i / resolution)));
                }
            }

            points.Add(transform.InverseTransformPoint(transforms[^1].position));

            GetComponent<LineRenderer>().positionCount = points.Count;
            GetComponent<LineRenderer>().SetPositions(points.ToArray());
        }
    }
}