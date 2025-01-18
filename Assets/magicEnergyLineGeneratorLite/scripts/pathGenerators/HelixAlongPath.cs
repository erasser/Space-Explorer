using System.Collections.Generic;
using UnityEngine;

namespace soulercoasterLite.scripts.pathGenerators {
    [RequireComponent(typeof(LineRenderer))]
    public class HelixAlongPath : MonoBehaviour {
        [Header("Generates a helix along the path of the chosen LineRenderer.")]
        [Tooltip("LineRenderer to generate Helix around")]
        public LineRenderer referencePath;

        [Tooltip("Radius at the start")] public float startRadius = 10;
        [Tooltip("Radius at the end")] public float endRadius = 10;

        [Tooltip("Number of points per path point to generate")]
        public float resolution = 2;

        [Tooltip("Number of rotations in the helix")]
        public float rotations = 1;

        [Tooltip("Radius multiplies with the width curve of the given LineRenderer")]
        public bool radiusWithWidthCurve = false;

        [Tooltip("Starting rotation")]
        public int rotationOffset = 0;
        public bool liveUpdate = true;

        public void OnValidate() {
            this.setupLineRenderer();
            if (liveUpdate) {
                generate();
            }
        }


        [ContextMenu("generate")]
        public void generate() {
            if (referencePath == null) {
                return;
            }
            var path = new Vector3[referencePath.positionCount];
            referencePath.GetPositions(path);

            var helixFrequency = (360f / (path.Length)) * rotations;
            var currentPoint = path[0];
            var i = 1;
            var points = new List<Vector3>();
            while (i < path.Length) {
                var nextPoint = path[i];
                var generalDirection = nextPoint - currentPoint;
                var nextDirection = Vector3.zero;
                if ((i + 1) < path.Length) {
                    nextDirection = path[i + 1] - nextPoint;
                }

                var crossProductBase = Vector3.up;
                var lastDeltaDirection = generalDirection;


                for (var stepIndex = 0f; stepIndex < (resolution); stepIndex++) {
                    Vector3 baseDirection;
                    if (nextDirection != Vector3.zero) {
                        baseDirection = Vector3.Lerp(generalDirection, nextDirection, stepIndex / resolution);
                    }
                    else {
                        baseDirection = generalDirection;
                    }

                    var basePoint = currentPoint + (generalDirection * (stepIndex / (resolution)));

                    crossProductBase = (Quaternion.FromToRotation(lastDeltaDirection, baseDirection) * crossProductBase)
                        .normalized;
                    var perpendicular = Vector3.Normalize(Vector3.Cross(baseDirection, crossProductBase));
                    var circleVector = Quaternion.AngleAxis(
                                           i * helixFrequency + (helixFrequency * (stepIndex / resolution))+rotationOffset,
                                           baseDirection) *
                                       perpendicular;

                    var nextHelixPoint =
                        basePoint + (circleVector * radius(path, (int)((i - 1) * resolution + stepIndex)));
                    points.Add(nextHelixPoint);
                }

                currentPoint = nextPoint;

                i++;
            }

            GetComponent<LineRenderer>().positionCount = points.Count;
            GetComponent<LineRenderer>().SetPositions(points.ToArray());
        }

        private float radius(IReadOnlyCollection<Vector3> path, int i) {

            return Mathf.Lerp(startRadius, endRadius, (i / ((path.Count - 1) * resolution))) *
                   radiusMultiplier(1f*i / ((path.Count - 1) * resolution));
        }

        private float radiusMultiplier(float time) {
            if (radiusWithWidthCurve) {
                return referencePath.widthCurve.Evaluate(time);
            }

            return 1;
        }
    }
}