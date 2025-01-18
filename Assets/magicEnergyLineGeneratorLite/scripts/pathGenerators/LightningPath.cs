using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace soulercoasterLite.scripts.pathGenerators {
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteInEditMode]
    public class LightningPath : MonoBehaviour {
        [Header("Lightning Path: Generates a random path between points")]
        public Transform origin;

        public Transform destination;
        public float arcStrength = 0.5f;
        public float stepSize = 2;
        public float animateSpeed = 0.1f;
        public bool animateOnStart = true;

        public void OnValidate() {
            this.setupLineRenderer();
        }

        public void Start() {
            if (animateOnStart) {
                StartCoroutine(animateArc(animateSpeed));
            }
        }

        [ContextMenu("generate()")]
        public void generateArc() {
            generateRandomArc();
        }

        private void generateRandomArc() {
            var currentPoint = origin.position;
            var destinationPoint = destination.position;
            var generalDirection = Vector3.Normalize(destinationPoint - currentPoint);

            var i = 1;
            var points = new List<Vector3> { currentPoint };
            while (Vector3.Distance(currentPoint, destinationPoint) > stepSize) {
                var randomVector =
                    Vector3.Normalize(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
                var currentDirection = Vector3.Normalize(destinationPoint - currentPoint);
                var randomDirectionVector = Vector3.Cross(generalDirection, randomVector);
                var nextPoint = currentPoint + currentDirection * stepSize +
                                randomDirectionVector * arcStrength * stepSize;
                points.Add(nextPoint);
                i++;
                currentPoint = nextPoint;
                if (i > 1000) {
                    break;
                }
            }

            points.Add(destinationPoint);

            GetComponent<LineRenderer>().positionCount = points.Count;
            GetComponent<LineRenderer>().SetPositions(points.ToArray());
        }

        [ContextMenu("animateArk()")]
        public void startArc() {
            StartCoroutine(animateArc(animateSpeed));
        }

        private IEnumerator animateArc(float speed) {
            var wait = new WaitForSeconds(speed);
            var soulercoaster = GetComponent<SoulerCoaster>();
            while (true) {
                generateArc();
                if (soulercoaster != null) {
                    soulercoaster.generate();
                }

                yield return wait;
            }
        }
    }
}