using System;
using System.Collections.Generic;
using UnityEngine;

namespace soulercoasterLite.scripts.pathGenerators {
    [RequireComponent(typeof(LineRenderer))]
    public class CircularSinusLine : MonoBehaviour {
        [Header("Generates a circle path with a sinus distortion")] [Tooltip("Direction of the path")]
        public Vector3 axis = Vector3.up;
        [Tooltip("Number of points to generate")]
        public long resolution = 20;
        [Tooltip("Radius of the circle")]
        public long radius = 10;
        [Tooltip("Number of waves in the circle")]
        public long sinusFrequency = 1;
        [Tooltip("Strength of waves in the circle")]
        public float sinusStrength = 1;
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
            var perpendicular = Vector3.Normalize(Vector3.Cross(axis, Vector3.up));
            if (perpendicular == Vector3.zero) {
                perpendicular = Vector3.Normalize(Vector3.Cross(axis, Vector3.forward));
            }

            var degreeDelta = 360 / (resolution*1f );
            var sinusDegreeDelta = ((Math.PI*2) / (resolution-2))*sinusFrequency;

            while (i < resolution) {
                var sinusDegreeScale = (float)Math.Sin(i * sinusDegreeDelta) * sinusStrength;
                var nextPoint = Quaternion.AngleAxis(i * degreeDelta, axis) * perpendicular * radius +
                                Vector3.Scale(axis, new Vector3(sinusDegreeScale, sinusDegreeScale, sinusDegreeScale));
                points.Add(nextPoint);
                i++;
            }

            GetComponent<LineRenderer>().positionCount = points.Count;
            GetComponent<LineRenderer>().SetPositions(points.ToArray());
        }
    }
}