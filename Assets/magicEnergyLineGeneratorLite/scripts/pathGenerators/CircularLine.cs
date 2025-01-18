using System.Collections.Generic;
using UnityEngine;

namespace soulercoasterLite.scripts.pathGenerators {
    [RequireComponent(typeof(LineRenderer))]
    public class CircularLine : MonoBehaviour {
        [Header("Generates a circle path")] [Tooltip("Direction of the path")]
        public Vector3 axis = Vector3.up;

        [Tooltip("Number of points to generate")]
        public long resolution = 20;

        [Tooltip("Radius of the circle")] public long radius = 10;
        [Tooltip("circular section")] public long arc = 360;
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

            var degreeDelta = arc / (resolution - 1f);

            while (i < resolution) {
                var nextPoint = Quaternion.AngleAxis(i * degreeDelta, axis) * perpendicular * radius;
                points.Add(nextPoint);
                i++;
            }

            GetComponent<LineRenderer>().positionCount = points.Count;
            GetComponent<LineRenderer>().SetPositions(points.ToArray());
        }
    }
}