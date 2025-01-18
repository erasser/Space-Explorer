using System;
using System.Collections.Generic;
using UnityEngine;

namespace soulercoasterLite.scripts.particle {
    public enum CoasterParticleSpawnMode {
        [Tooltip("Particles spawn at the start of the path.")]
        Start,
        [Tooltip("Particles spawn at the end of the path.")]
        End,
        [Tooltip("Particle Spawn location is updated each frame. ")]
        Random
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleFollowPath : MonoBehaviour {
        private ParticleSystem particleSystem;
        [Tooltip("Path the particles should follow")]
        public LineRenderer pathToFollow;
        private ParticleSystem.Particle[] particles;
        private Vector3 firstPos;
        private Vector3 secondPos;
        private float subPos;
        [Tooltip("Allows different spawn modes")]
        public CoasterParticleSpawnMode mode = CoasterParticleSpawnMode.Start;

        [Tooltip("Sets the strength to conform to the LineRenderer path.")] [Range(0, 1)]
        public float force = 0.5f;

        [Tooltip("Prevent particle jumps in non-closed paths.")]
        public bool loopPath = true;

        private Dictionary<uint, float> lastPathPosition = new();
        private Dictionary<uint, float> startVelocity = new();
        private float currentShapePathPosition;


        private void Start() {
            particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnValidate() {
            if (pathToFollow == null) {
                return;
            }

            if (particleSystem == null) {
                particleSystem = GetComponent<ParticleSystem>();
            }

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
            if (mode != CoasterParticleSpawnMode.End) {
                shape.position =
                    transform.InverseTransformPoint(pathToFollow.transform.TransformPoint(pathToFollow.GetPosition(0)));
                currentShapePathPosition = 0;
            }
            else {
                shape.position =
                    transform.InverseTransformPoint(
                        pathToFollow.transform.TransformPoint(
                            pathToFollow.GetPosition(pathToFollow.positionCount - 1)));
                currentShapePathPosition = pathToFollow.positionCount - 1;
            }
        }

        private void Update() {
            if (pathToFollow == null) {
                return;
            }

            if (particles == null || particles.Length < particleSystem.main.maxParticles) {
                particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
            }

            particleSystem.GetParticles(particles);

            var numParticlesAlive = particleSystem.GetParticles(particles);
            var positionCount = pathToFollow.positionCount;

            for (var i = 0; i < numParticlesAlive; i++) {
                var particle = particles[i];
                var id = particle.randomSeed;

                var velocity = 0f;
                if (startVelocity.ContainsKey(id)) {
                    velocity = startVelocity[id];
                }
                else {
                    velocity = particle.velocity.y;
                    startVelocity[id] = velocity;
                }

                var positionOnPath = currentShapePathPosition;
                if (lastPathPosition.ContainsKey(id)) {
                    positionOnPath = lastPathPosition[id];
                }


                var projectedPath = (positionOnPath + velocity * Time.deltaTime);
                subPos = projectedPath - MathF.Truncate(projectedPath);

                if (projectedPath < 0) {
                    projectedPath += positionCount;
                }

                int nextPositionOnPath;
                if (velocity < 0) {
                    nextPositionOnPath = (int)MathF.Floor(projectedPath);
                }
                else {
                    nextPositionOnPath = (int)MathF.Ceiling(projectedPath);
                }


                nextPositionOnPath %= positionCount;

                if (!loopPath) {
                    if (
                        ((int)positionOnPath > (int)nextPositionOnPath && velocity > 0) ||
                        ((int)positionOnPath < (int)nextPositionOnPath && velocity < 0)
                    ) {
                        particles[i].remainingLifetime = 0f;
                        lastPathPosition.Remove(id);
                        startVelocity.Remove(id);
                    }
                }

                if (velocity < 0) {
                    firstPos = pathToFollow.GetPosition((int)MathF.Ceiling(projectedPath) % positionCount);
                }
                else {
                    firstPos = pathToFollow.GetPosition((int)MathF.Floor(projectedPath) % positionCount);
                }

                secondPos = pathToFollow.GetPosition(nextPositionOnPath);


                if (particle.remainingLifetime > 0) {
                    if (velocity > 0) {
                        particles[i].position =
                            Vector3.Lerp(particle.position, Vector3.Lerp(firstPos, secondPos, subPos), force);
                    }
                    else {
                        particles[i].position =
                            Vector3.Lerp(particle.position, Vector3.Lerp(secondPos, firstPos, subPos), force);
                    }
                }

                lastPathPosition[id] = projectedPath;
            }

            particleSystem.SetParticles(particles, numParticlesAlive);

            if (mode == CoasterParticleSpawnMode.Random) {
                var shape = particleSystem.shape;
                var position = (int)(UnityEngine.Random.value * (pathToFollow.positionCount - 1));
                shape.position =
                    transform.InverseTransformPoint(
                        pathToFollow.transform.TransformPoint(
                            pathToFollow.GetPosition(position)));
                currentShapePathPosition = position;
            }
        }
    }
}