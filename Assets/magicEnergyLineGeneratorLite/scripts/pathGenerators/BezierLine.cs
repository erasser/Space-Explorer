using System.Collections.Generic;
using UnityEngine;

namespace soulercoasterLite.scripts.pathGenerators {
    [RequireComponent(typeof(LineRenderer))]
    public class BezierLine : MonoBehaviour {
        [Header("Generates a bezier path using first, second, and third point.")]
        [Tooltip("Number of points to generate")]
        public long resolution = 20;

        public Vector3 first;
        public Vector3 second;
        public Vector3 third;

        public bool liveUpdate = true;

        public void OnValidate() {
            this.setupLineRenderer();
            if (liveUpdate) {
                generate();
            }
        }

        [ContextMenu("generate()")]
        public void generate() {
            var i = 0;
            var points = new List<Vector3>();

            while (i < resolution) {
                var t = (i / (resolution * 1f));
                float oneMinusT = 1f - t;
                var nextPoint = oneMinusT * oneMinusT * first +
                                2f * oneMinusT * t * second +
                                t * t * third;
                points.Add(nextPoint);
                i++;
            }

            GetComponent<LineRenderer>().positionCount = points.Count;
            GetComponent<LineRenderer>().SetPositions(points.ToArray());
        }
    }
}