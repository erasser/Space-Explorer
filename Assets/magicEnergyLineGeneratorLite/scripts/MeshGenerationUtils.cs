using System;
using UnityEngine;

namespace soulercoasterLite.scripts {
    public static class QuadPathUtils {
        /**
         * Function to create a regular ngon plane
         */
        public static void pushQuadPath(Vector3[] positions, MeshBuilder builder, bool isLooped,
            Func<float, float> getWidth,
            float rotateAngle,
            float quadWidthFactor,
            int quadIndex,
            int quadCount, float distanceToBaseFactor = 1f, bool pushOnlyVertices = false,
            bool newVerticesForEachEdge = false, bool dontSkewNormals = false, Vector3? normalVector = null,
            bool cheapEdges = false,
            float skew = 0f, float planeAngle = 0f, bool oneSided = false) {
            if (positions.Length < 2) {
                Debug.LogWarning("Can't create mesh for 1 entry paths, add path points");
                return;
            }

            var currentRotateAngle = rotateAngle;

            var currentPosition = positions[0];
            var nextPosition = positions[1];
            var deltaDirection = nextPosition - currentPosition;
            var nextDeltaDirection = nextPosition - currentPosition;
            if (isLooped) {
                deltaDirection = currentPosition - positions[^1];
            }

            var crossProductBase = Vector3.up;

            if (normalVector != null && normalVector != Vector3.zero) {
                crossProductBase = normalVector.Value;
            }

            var position1 = createPushQuadPosition(getWidth(0), deltaDirection, crossProductBase, currentRotateAngle,
                currentPosition);

            var nextCrossProductBase =
                (Quaternion.FromToRotation(deltaDirection, nextDeltaDirection) * crossProductBase);
            var position2 = createPushQuadPosition(getWidth(0), nextDeltaDirection, nextCrossProductBase,
                currentRotateAngle, nextPosition);
            if (!cheapEdges) {
                pushNextQuad(builder, position1, position2, quadWidthFactor, newVerticesForEachEdge, initial: true,
                    distanceToBase: distanceToBaseFactor, planeAngle: planeAngle, dontSkewNormals: dontSkewNormals);
            }
            else {
                pushNextQuadCheap(builder, position1, quadWidthFactor, newVerticesForEachEdge, initial: true,
                    distanceToBase: distanceToBaseFactor, planeAngle: planeAngle, dontSkewNormals: dontSkewNormals);
            }

            if (!pushOnlyVertices) {
                pushUvAndTriangles(builder, 0, positions.Length, quadIndex, quadCount, newVerticesForEachEdge, true,
                    dontSkewNormals, oneSided);
            }

            pushNormalsAndTangents(builder, newVerticesForEachEdge, dontSkewNormals, deltaDirection,
                position1.quadDirection,
                nextPosition, initial: true);
            currentRotateAngle += skew;

            var lastPosition = positions[0];

            var startIndex = builder.i;
            var i = builder.i + 1;
            var lastDeltaDirection = deltaDirection;
            while ((i - startIndex) < (positions.Length)) {
                currentPosition = positions[i - startIndex];

                nextPosition = positions[0];
                if ((i - startIndex + 1) < positions.Length) {
                    nextPosition = positions[i + 1 - startIndex];
                }

                if (nextPosition == currentPosition) {
                    i++;
                    Debug.LogError($"Duplicate point at {i - startIndex}, skipping point");
                    continue;
                }

                deltaDirection = currentPosition - lastPosition;
                nextDeltaDirection = nextPosition - currentPosition;

                crossProductBase = (Quaternion.FromToRotation(lastDeltaDirection, deltaDirection) * crossProductBase);
                var width = getWidth((i - startIndex * 1f) / positions.Length);

                position1 = createPushQuadPosition(width, deltaDirection, crossProductBase, currentRotateAngle,
                    currentPosition, position1);

                if (!cheapEdges) {
                    nextCrossProductBase =
                        (Quaternion.FromToRotation(deltaDirection, nextDeltaDirection) * crossProductBase);
                    position2 = createPushQuadPosition(width, nextDeltaDirection, nextCrossProductBase,
                        currentRotateAngle, nextPosition, position2);
                    pushNextQuad(builder, position1, position2, quadWidthFactor, newVerticesForEachEdge, initial: false,
                        distanceToBase: distanceToBaseFactor, planeAngle: planeAngle, dontSkewNormals: dontSkewNormals);
                }
                else {
                    pushNextQuadCheap(builder, position1, quadWidthFactor, newVerticesForEachEdge, initial: false,
                        distanceToBase: distanceToBaseFactor, planeAngle: planeAngle, dontSkewNormals: dontSkewNormals);
                }

                if (!pushOnlyVertices) {
                    pushUvAndTriangles(builder, i - startIndex, positions.Length, quadIndex, quadCount,
                        newVerticesForEachEdge, false, dontSkewNormals, oneSided);
                }

                currentRotateAngle += skew;

                pushNormalsAndTangents(builder, newVerticesForEachEdge, dontSkewNormals, deltaDirection,
                    position1.quadDirection, currentPosition);

                lastDeltaDirection = deltaDirection;
                lastPosition = currentPosition;
                i++;
            }


            if (isLooped) {
                currentPosition = positions[0];
                nextPosition = positions[1];

                deltaDirection = currentPosition - lastPosition;
                nextDeltaDirection = nextPosition - currentPosition;

                crossProductBase = (Quaternion.FromToRotation(lastDeltaDirection, deltaDirection) * crossProductBase);
                position1 = createPushQuadPosition(getWidth(1), deltaDirection, crossProductBase, currentRotateAngle,
                    currentPosition, position1);

                if (!cheapEdges) {
                    nextCrossProductBase =
                        (Quaternion.FromToRotation(deltaDirection, nextDeltaDirection) * crossProductBase);
                    position2 = createPushQuadPosition(getWidth(1), nextDeltaDirection, nextCrossProductBase,
                        currentRotateAngle, nextPosition, position2);

                    pushNextQuad(builder, position1, position2, quadWidthFactor, newVerticesForEachEdge, initial: false,
                        distanceToBase: distanceToBaseFactor, planeAngle: planeAngle, dontSkewNormals);
                }
                else {
                    pushNextQuadCheap(builder, position1, quadWidthFactor, newVerticesForEachEdge, initial: false,
                        distanceToBase: distanceToBaseFactor, planeAngle: planeAngle, dontSkewNormals: dontSkewNormals);
                }

                if (!pushOnlyVertices) {
                    pushUvAndTriangles(builder, i - startIndex, positions.Length, quadIndex, quadCount,
                        newVerticesForEachEdge, false, dontSkewNormals, oneSided);
                }

                pushNormalsAndTangents(builder, newVerticesForEachEdge, dontSkewNormals, deltaDirection,
                    position1.quadDirection,
                    currentPosition);
                i++;
            }

            builder.i = i;
        }

        private static PushQuadPosition createPushQuadPosition(float width, Vector3 deltaDirection,
            Vector3 crossProductBase, float currentRotateAngle, Vector3 position, PushQuadPosition? existing = null) {
            var nextQuadDirection = Vector3.Cross(crossProductBase, deltaDirection).normalized;
            var nextRotateQuaternion = Quaternion.AngleAxis(currentRotateAngle, deltaDirection);
            var nextQuadDelta = nextRotateQuaternion * Vector3.Cross(nextQuadDirection, deltaDirection).normalized;
            nextQuadDirection = nextRotateQuaternion * nextQuadDirection;
            if (existing == null) {
                return new PushQuadPosition {
                    position = position,
                    quadDelta = nextQuadDelta,
                    quadDirection = nextQuadDirection,
                    width = width,
                    deltaDirection = deltaDirection
                };
            }

            var quadPosition = existing.Value;
            quadPosition.position = position;
            quadPosition.quadDelta = nextQuadDelta;
            quadPosition.quadDirection = nextQuadDirection;
            quadPosition.width = width;
            quadPosition.deltaDirection = deltaDirection;
            return quadPosition;
        }

        private struct PushQuadPosition {
            public Vector3 position;
            public Vector3 quadDelta;
            public Vector3 quadDirection;
            public Vector3 deltaDirection;
            public float width;
        }

        private static void pushNormalsAndTangents(MeshBuilder builder, bool newVerticesForEachEdge,
            bool dontSkewNormals,
            Vector3 deltaDirection, Vector3 quadDirection, Vector3 nextPosition, bool initial = false) {
            if (!newVerticesForEachEdge) {
                if (dontSkewNormals) {
                    // We need to duplicated all vertices
                    builder.normalList.Add(Vector3.Cross(deltaDirection, quadDirection).normalized);
                    builder.normalList.Add(Vector3.Cross(deltaDirection, quadDirection).normalized);
                    builder.normalList.Add(Vector3.Cross(-deltaDirection, quadDirection).normalized);
                    builder.normalList.Add(Vector3.Cross(-deltaDirection, quadDirection).normalized);
                    builder.tangentList.Add(deltaDirection);
                    builder.tangentList.Add(deltaDirection);
                    builder.tangentList.Add(deltaDirection);
                    builder.tangentList.Add(deltaDirection);
                }
                else {
                    builder.normalList.Add(nextPosition);
                    builder.normalList.Add(nextPosition);
                    builder.tangentList.Add(deltaDirection);
                    builder.tangentList.Add(deltaDirection);
                }
            }
            else {
                if (dontSkewNormals) {
                    if (!initial) {
                        builder.normalList.Add(Vector3.Cross(deltaDirection, quadDirection).normalized);
                        builder.normalList.Add(Vector3.Cross(deltaDirection, quadDirection).normalized);
                        builder.normalList.Add(Vector3.Cross(-deltaDirection, quadDirection).normalized);
                        builder.normalList.Add(Vector3.Cross(-deltaDirection, quadDirection).normalized);
                        builder.tangentList.Add(deltaDirection);
                        builder.tangentList.Add(deltaDirection);
                        builder.tangentList.Add(deltaDirection);
                        builder.tangentList.Add(deltaDirection);
                    }

                    builder.normalList.Add(Vector3.Cross(deltaDirection, quadDirection).normalized);
                    builder.normalList.Add(Vector3.Cross(deltaDirection, quadDirection).normalized);
                    builder.normalList.Add(Vector3.Cross(-deltaDirection, quadDirection).normalized);
                    builder.normalList.Add(Vector3.Cross(-deltaDirection, quadDirection).normalized);
                    builder.tangentList.Add(deltaDirection);
                    builder.tangentList.Add(deltaDirection);
                    builder.tangentList.Add(deltaDirection);
                    builder.tangentList.Add(deltaDirection);
                }
                else {
                    builder.normalList.Add(nextPosition);
                    builder.normalList.Add(nextPosition);
                    builder.tangentList.Add(deltaDirection);
                    builder.tangentList.Add(deltaDirection);
                    if (!initial) {
                        builder.normalList.Add(nextPosition);
                        builder.normalList.Add(nextPosition);
                        builder.tangentList.Add(deltaDirection);
                        builder.tangentList.Add(deltaDirection);
                    }
                }
            }
        }

        /**
         * Theoretically this can fail if you supply wrong lines. This would make the mesh look odd
         */
        private static Vector3? lineLineIntersection(Vector3 linePoint1,
            Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {
            if (Vector3.Angle(lineVec1, lineVec2) > 175) {
                Debug.LogWarning("Falling back to cheap edges");
                return null;
            }

            var lineVec3 = linePoint2 - linePoint1;
            var crossVec1And2 = Vector3.Cross(lineVec1, lineVec2);
            var crossVec3And2 = Vector3.Cross(lineVec3, lineVec2);

            var s = Vector3.Dot(crossVec3And2, crossVec1And2)
                    / crossVec1And2.sqrMagnitude;
            return linePoint1 + (lineVec1 * s);
        }

        // Push a quad by adding 2 new vectors and 4 new faces to vector and trianglelist
        private static void pushNextQuad(MeshBuilder builder,
            PushQuadPosition position,
            PushQuadPosition nextPosition,
            float quadWidthFactor, bool newVerticesForEachEdge,
            bool initial = false, float distanceToBase = 0f, float planeAngle = 0f, bool dontSkewNormals = false
        ) {
            var scaledDirection = Quaternion.AngleAxis(planeAngle, position.deltaDirection) * position.quadDirection *
                                  (position.width * quadWidthFactor);
            var scaledNextDirection = Quaternion.AngleAxis(planeAngle, nextPosition.deltaDirection) *
                                      nextPosition.quadDirection * (nextPosition.width * quadWidthFactor);
            var alongVector1 = Vector3.Cross(position.quadDelta, position.quadDirection);
            var alongVector2 = Vector3.Cross(nextPosition.quadDelta, nextPosition.quadDirection);
            var base1 = position.position - position.quadDelta * (distanceToBase * position.width) / 2;
            var intersect1 = base1 + scaledDirection;
            var intersect2 = base1 - scaledDirection;
            if (alongVector1 != alongVector2) {
                var base2 = nextPosition.position - nextPosition.quadDelta * (distanceToBase * nextPosition.width) / 2;
                intersect1 = lineLineIntersection(base1 + scaledDirection, alongVector1,
                    base2 + scaledNextDirection, alongVector2) ?? base1 + scaledDirection;
                intersect2 = lineLineIntersection(base1 - scaledDirection, alongVector1,
                    base2 - scaledNextDirection, alongVector2) ?? base1 + scaledDirection;
            }

            if (newVerticesForEachEdge && !initial) {
                if (dontSkewNormals) {
                    builder.vectorList.Add(builder.vectorList[^4]);
                    builder.vectorList.Add(builder.vectorList[^4]);
                    builder.vectorList.Add(builder.vectorList[^4]);
                    builder.vectorList.Add(builder.vectorList[^4]);
                }
                else {
                    builder.vectorList.Add(builder.vectorList[^2]);
                    builder.vectorList.Add(builder.vectorList[^2]);
                }
            }

            if (dontSkewNormals) {
                builder.vectorList.Add(intersect1);
                builder.vectorList.Add(intersect2);
                builder.vectorList.Add(intersect1);
                builder.vectorList.Add(intersect2);
            }
            else {
                builder.vectorList.Add(intersect1);
                builder.vectorList.Add(intersect2);
            }
        }

        private static void pushUvAndTriangles(MeshBuilder builder, int index, int length, int quadIndex, int quadCount,
            bool newVerticesForEachEdge, bool initial, bool dontSkewNormals, bool oneSided) {
            var time = index / (length * 1f);

            builder.uvCoordinates1.Add(new Vector2(0, time));
            builder.uvCoordinates1.Add(new Vector2(1, time));

            builder.uvCoordinates2.Add(new Vector2((quadIndex + 0f) / quadCount, time));
            builder.uvCoordinates2.Add(new Vector2((quadIndex + 1f) / quadCount, time));
            if (dontSkewNormals) {
                builder.uvCoordinates1.Add(new Vector2(0, time));
                builder.uvCoordinates1.Add(new Vector2(1, time));
                builder.uvCoordinates2.Add(new Vector2((quadIndex + 0f) / quadCount, time));
                builder.uvCoordinates2.Add(new Vector2((quadIndex + 1f) / quadCount, time));
            }

            if (newVerticesForEachEdge && !initial) {
                var time2 = (index + 1) / (length * 1f);
                builder.uvCoordinates1.Add(new Vector2(0, time2));
                builder.uvCoordinates1.Add(new Vector2(1, time2));

                builder.uvCoordinates2.Add(new Vector2((quadIndex + 0f) / quadCount, time2));
                builder.uvCoordinates2.Add(new Vector2((quadIndex + 1f) / quadCount, time2));
                if (dontSkewNormals) {
                    builder.uvCoordinates1.Add(new Vector2(0, time2));
                    builder.uvCoordinates1.Add(new Vector2(1, time2));

                    builder.uvCoordinates2.Add(new Vector2((quadIndex + 0f) / quadCount, time2));
                    builder.uvCoordinates2.Add(new Vector2((quadIndex + 1f) / quadCount, time2));
                }
            }

            if (initial) {
                return;
            }

            if (dontSkewNormals) {
                // use the same vertices for both sides
                var baseIndex = builder.vectorList.Count - 8;
                if (newVerticesForEachEdge) {
                    //baseIndex = builder.vectorList.Count - 12;
                }

                builder.triangleList.Add(baseIndex + 1);
                builder.triangleList.Add(baseIndex + 5);
                builder.triangleList.Add(baseIndex + 0);

                builder.triangleList.Add(baseIndex + 4);
                builder.triangleList.Add(baseIndex + 0);
                builder.triangleList.Add(baseIndex + 5);

                if (!oneSided) {
                    builder.triangleList.Add(baseIndex + 2);
                    builder.triangleList.Add(baseIndex + 6);
                    builder.triangleList.Add(baseIndex + 7);

                    builder.triangleList.Add(baseIndex + 2);
                    builder.triangleList.Add(baseIndex + 7);
                    builder.triangleList.Add(baseIndex + 3);
                }
            }
            else {
                // use the same vertices for both sides
                var baseIndex = builder.vectorList.Count - 2;

                builder.triangleList.Add(baseIndex - 2);
                builder.triangleList.Add(baseIndex - 1);
                builder.triangleList.Add(baseIndex);
                builder.triangleList.Add(baseIndex - 1);
                builder.triangleList.Add(baseIndex + 1);
                builder.triangleList.Add(baseIndex);


                if (!oneSided) {
                    builder.triangleList.Add(baseIndex);
                    builder.triangleList.Add(baseIndex - 1);
                    builder.triangleList.Add(baseIndex - 2);
                    builder.triangleList.Add(baseIndex);
                    builder.triangleList.Add(baseIndex + 1);
                    builder.triangleList.Add(baseIndex - 1);
                }
            }
        }

        private static void pushNextQuadCheap(MeshBuilder builder,
            PushQuadPosition position,
            float quadWidthFactor, bool newVerticesForEachEdge, float planeAngle,
            bool initial = false, float distanceToBase = 0f, bool dontSkewNormals = false
        ) {
            var scaledDirection = Quaternion.AngleAxis(planeAngle, position.deltaDirection) * position.quadDirection *
                                  (position.width * quadWidthFactor);
            var base1 = position.position - position.quadDelta * (distanceToBase * position.width) / 2;

            if (newVerticesForEachEdge && !initial) {
                if (dontSkewNormals) {
                    builder.vectorList.Add(builder.vectorList[^4]);
                    builder.vectorList.Add(builder.vectorList[^4]);
                    builder.vectorList.Add(builder.vectorList[^4]);
                    builder.vectorList.Add(builder.vectorList[^4]);
                }
                else {
                    builder.vectorList.Add(builder.vectorList[^2]);
                    builder.vectorList.Add(builder.vectorList[^2]);
                }
            }

            builder.vectorList.Add(base1 + scaledDirection);
            builder.vectorList.Add(base1 - scaledDirection);
            if (dontSkewNormals) {
                builder.vectorList.Add(base1 + scaledDirection);
                builder.vectorList.Add(base1 - scaledDirection);
            }
        }
    }
}