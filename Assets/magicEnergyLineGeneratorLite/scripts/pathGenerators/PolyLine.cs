using System.Collections.Generic;
using UnityEngine;

namespace soulercoasterLite.scripts.pathGenerators {
    [RequireComponent(typeof(LineRenderer))]
    public class PolyLine : MonoBehaviour {
        [Header("Generates a path along a regular polygon")] [Tooltip("Number of edges points to generate")]
        public long resolution = 6;

        [Tooltip("Initial number of edges of the regular polygon")]
        public float startEdgeCount = 6;

        [Tooltip("Changes the length of each side along the polygon.")]
        public float angleDelta = 0;

        [Tooltip("Changes the height of each point along the polygon.")]
        public float heightDelta = 0;

        [Tooltip("Radius of the polygon (distance to middle at edge)")]
        public float startRadius = 10;

        [Tooltip("Changes the radius of each point along the polygon.")]
        public float radiusDelta = 0;

        [Tooltip("Number of points on each side of the polygon. Useful if you want to use it with Ribbon.")]
        public float sidePointCount = 0;


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
            var degreeDelta = 360f / startEdgeCount;

            var perpendicular = Vector3.Normalize(Vector3.Cross(Vector3.forward, Vector3.up));
            if (perpendicular == Vector3.zero) {
                perpendicular = Vector3.Normalize(Vector3.Cross(Vector3.forward, Vector3.forward));
            }

            var radius = startRadius;

            var height = 0f;
            var lastPoint = Vector3.zero;
            while (i < resolution) {
                var nextPoint = Quaternion.AngleAxis(i * degreeDelta, Vector3.forward) * perpendicular * radius;
                nextPoint.z = height;
                if (lastPoint != Vector3.zero) {
                    for (var k = 0f; k < sidePointCount; k++) {
                        points.Add(Vector3.Lerp(lastPoint, nextPoint, (k + 1) / (sidePointCount + 1)));
                    }
                }

                points.Add(nextPoint);
                lastPoint = nextPoint;
                degreeDelta += angleDelta;
                height += heightDelta;
                radius += radiusDelta;
                i++;
            }

            GetComponent<LineRenderer>().positionCount = points.Count;
            GetComponent<LineRenderer>().SetPositions(points.ToArray());
        }
    }
}