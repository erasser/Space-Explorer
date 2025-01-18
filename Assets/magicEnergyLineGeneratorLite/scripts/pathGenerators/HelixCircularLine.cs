using System.Collections.Generic;
using UnityEngine;

namespace soulercoasterLite.scripts.pathGenerators {
    [RequireComponent(typeof(LineRenderer))]
    public class HelixCircularLine : MonoBehaviour {
        [Header("Generates a helix along a circular path")] [Tooltip("Direction of the path")]
        public Vector3 axis = Vector3.up;

        [Tooltip("Number of points to generate")]
        public long resolution = 200;

        [Tooltip("Radius of the base circle")] public long radius = 10;

        [Tooltip("Radius of the helix on circle")]
        public long helixRadius = 1;

        [Tooltip("Rotations of the helix along the circle")]
        public long helixRounds = 3;
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

            var degreeDelta = 360f / (resolution );
            var helixDegreeDelta = (360f * helixRounds) / (resolution );

            while (i < resolution) {
                var basePoint = Quaternion.AngleAxis(i * degreeDelta, axis) * perpendicular * radius;
                var circleDirection = ((Quaternion.AngleAxis((i + 1) * degreeDelta, axis) * perpendicular * radius) - basePoint).normalized;
                var nextPoint = basePoint +
                                Quaternion.AngleAxis(i * helixDegreeDelta, circleDirection) * basePoint.normalized * helixRadius;
                points.Add(nextPoint);
                i++;
            }

            GetComponent<LineRenderer>().positionCount = points.Count;
            GetComponent<LineRenderer>().SetPositions(points.ToArray());
        }
    }
}