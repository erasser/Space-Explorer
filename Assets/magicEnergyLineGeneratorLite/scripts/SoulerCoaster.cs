using System;
using soulercoaster.scripts;
using UnityEditor;
using UnityEngine;

namespace soulercoasterLite.scripts {
    [RequireComponent(typeof(LineRenderer),
        typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class SoulerCoaster : MonoBehaviour {
        private AnimationCurve lineRendererWidthCurve;
        private Mesh mesh;

        [Tooltip("Base width of the soulercoaster.")]
        public float width = 2;

        [Tooltip("Skew the rotation of the planes in the mesh.")]
        public float skew = 0f;

        [Tooltip("Use the width curve of the LineRenderer component")]
        public bool useLineRendererWidthCurve = false;

        [Tooltip("Generates mesh when the object is enabled.")]
        public bool generateMeshOnEnable = true;

        [Tooltip("This makes mesh generation faster, but compromises on the edge form")]
        public bool cheapEdges = true;


        [Tooltip("Generates a mesh with correct normals. Can't be used with the SoulerCoaster shader.")]
        public bool dontSkewNormals = false;

        [Tooltip("You can use this to rotate the generated mesh along the path.")]
        public Vector3 normalVector = new(0, 1, 0);

        public SoulerCoasterType type = SoulerCoasterType.SOULERCOASTER;

        [Tooltip("Generate separate vertex pairs for each path segment")]
        public bool newVerticeForEachEdge = false;

        [Tooltip("Only generate faces for one direction. Useful if you want to use the mesh for lightning.")]
        public bool oneSided = false;

        [Tooltip("Prevents the generated mesh from saving into a scene. This can help lower the size scenes take up. ")]
        public bool preventMeshSaving = false;

        [Header("Only for SOULERCASTER_NGON")] [Tooltip("Number of edges the ngon should have")]
        public int nGonCount = 3;

        [Tooltip("Used to project the planes outwards the ngon")]
        public float quadWidthFactor = 1;

        private bool shouldRender = false;

        public void OnEnable() {
            if (generateMeshOnEnable) {
                shouldRender = true;
            }
        }

        public void OnValidate() {
            if (generateMeshOnEnable) {
                shouldRender = true;
            }
        }

        private void Update() {
            if (shouldRender) {
                try {
                    generate();
                }
                catch (Exception e) {
                    Debug.LogError(e);
                }

                shouldRender = false;
            }
        }

        public Mesh getMesh() {
            if (mesh == null) {
                generate();
            }

            return mesh;
        }

#if UNITY_EDITOR
        [ContextMenu("saveMesh()")]
        private void saveMesh() {
            if (mesh == null) {
                generate();
            }

            ProjectWindowUtil.CreateAsset(mesh, "soulercoaster.asset");
            AssetDatabase.SaveAssets();
        }
#endif
        [ContextMenu("generate()")]
        public void generate() {
            var positions = new Vector3[GetComponent<LineRenderer>().positionCount];
            GetComponent<LineRenderer>().GetPositions(positions);
            var isLooped = GetComponent<LineRenderer>().loop;
            lineRendererWidthCurve = GetComponent<LineRenderer>().widthCurve;

            var builder = new MeshBuilder();

            var quadIndex = 0;
            if (type is SoulerCoasterType.VERTICAL_QUAD or SoulerCoasterType.SOULERCOASTER) {
                QuadPathUtils.pushQuadPath(positions, builder, isLooped,
                    getWidth, 90, 1,
                    0, 2, distanceToBaseFactor: 0f, newVerticesForEachEdge: newVerticeForEachEdge,
                    dontSkewNormals: dontSkewNormals, normalVector: normalVector, skew: skew, cheapEdges: cheapEdges,
                    oneSided: oneSided);
                quadIndex++;
            }

            if (type is SoulerCoasterType.HORZONTAL_QUAD or SoulerCoasterType.SOULERCOASTER) {
                QuadPathUtils.pushQuadPath(positions, builder, isLooped,
                    getWidth, 0, 1,
                    quadIndex, 2, distanceToBaseFactor: 0f, newVerticesForEachEdge: newVerticeForEachEdge,
                    dontSkewNormals: dontSkewNormals, normalVector: normalVector, skew: skew, cheapEdges: cheapEdges,
                    oneSided: oneSided);
            }

            if (type is SoulerCoasterType.SOULERCOASTER_N_GON) {
                var gonIndex = 0;
                var sideLength = (2f * Mathf.Tan((float)Math.PI / nGonCount)) * 0.25f;
                var angle = 360 / nGonCount;
                while (gonIndex < nGonCount) {
                    QuadPathUtils.pushQuadPath(positions, builder, isLooped,
                        getWidth, angle * gonIndex, sideLength * quadWidthFactor,
                        gonIndex, nGonCount, newVerticesForEachEdge: newVerticeForEachEdge,
                        dontSkewNormals: dontSkewNormals, normalVector: normalVector, skew: skew,
                        cheapEdges: cheapEdges, oneSided: oneSided);
                    gonIndex++;
                }
            }

            if (type is SoulerCoasterType.CUSTOM_PLANE_CONFIGURATION) {
                var gonIndex = 0;
                var planeConfig = GetComponent<CustomPlaneConfiguration>();
                if (planeConfig == null) {
                    planeConfig = gameObject.AddComponent<CustomPlaneConfiguration>();
                    planeConfig.planes.Add(new PlaneConfiguration {
                        width = 1
                    });
                }

                foreach (var plane in GetComponent<CustomPlaneConfiguration>().planes) {
                    QuadPathUtils.pushQuadPath(positions, builder, isLooped,
                        getWidth, plane.angle, plane.width * quadWidthFactor,
                        gonIndex, nGonCount, newVerticesForEachEdge: newVerticeForEachEdge,
                        dontSkewNormals: dontSkewNormals, normalVector: normalVector, skew: plane.skew,
                        distanceToBaseFactor: plane.distance,
                        cheapEdges: cheapEdges, planeAngle: plane.planeAngle, oneSided: oneSided);
                    gonIndex++;
                }
            }


            mesh = builder.build("soulercoaster", mesh);
            GetComponent<MeshFilter>().mesh = mesh;
        }

        // Reuse mesh builder
        public void generate(MeshBuilder builder, bool pushOnlyVertices = false) {
            builder.reset(pushOnlyVertices);
            var positions = new Vector3[GetComponent<LineRenderer>().positionCount];
            GetComponent<LineRenderer>().GetPositions(positions);
            var isLooped = GetComponent<LineRenderer>().loop;
            lineRendererWidthCurve = GetComponent<LineRenderer>().widthCurve;

            var quadIndex = 0;
            if (type is SoulerCoasterType.VERTICAL_QUAD or SoulerCoasterType.SOULERCOASTER) {
                QuadPathUtils.pushQuadPath(positions, builder, isLooped,
                    getWidth, 90, 1,
                    0, 2, distanceToBaseFactor: 0f, pushOnlyVertices: pushOnlyVertices,newVerticeForEachEdge,dontSkewNormals,normalVector,cheapEdges,skew,oneSided:oneSided);
                quadIndex++;
            }

            if (type is SoulerCoasterType.HORZONTAL_QUAD or SoulerCoasterType.SOULERCOASTER) {
                QuadPathUtils.pushQuadPath(positions, builder, isLooped,
                    getWidth, 0, 1,
                    quadIndex, 2, distanceToBaseFactor: 0f, pushOnlyVertices: pushOnlyVertices,newVerticeForEachEdge,dontSkewNormals,normalVector,cheapEdges,skew,oneSided:oneSided);
            }

            if (type is SoulerCoasterType.SOULERCOASTER_N_GON) {
                var gonIndex = 0;
                var sideLength = (2f * Mathf.Tan((float)Math.PI / nGonCount)) * 0.25f;
                var angle = 360 / nGonCount;
                while (gonIndex < nGonCount) {
                    QuadPathUtils.pushQuadPath(positions, builder, isLooped,
                        getWidth, angle * gonIndex, sideLength * quadWidthFactor,
                        gonIndex, nGonCount, 0f,pushOnlyVertices: pushOnlyVertices,newVerticeForEachEdge,dontSkewNormals,normalVector,cheapEdges,skew,oneSided:oneSided
                    );
                    gonIndex++;
                }
            }

            if (type is SoulerCoasterType.CUSTOM_PLANE_CONFIGURATION) {
                var gonIndex = 0;
                var planeConfig = GetComponent<CustomPlaneConfiguration>();
                if (planeConfig == null) {
                    planeConfig = gameObject.AddComponent<CustomPlaneConfiguration>();
                    planeConfig.planes.Add(new PlaneConfiguration {
                        width = 1
                    });
                }

                foreach (var plane in GetComponent<CustomPlaneConfiguration>().planes) {
                    QuadPathUtils.pushQuadPath(positions, builder, isLooped,
                        getWidth, plane.angle, plane.width * quadWidthFactor,
                        gonIndex, nGonCount, newVerticesForEachEdge: newVerticeForEachEdge,
                        dontSkewNormals: dontSkewNormals, normalVector: normalVector, skew: plane.skew,
                        distanceToBaseFactor: plane.distance,
                        cheapEdges: cheapEdges, oneSided: oneSided);
                    gonIndex++;
                }
            }


            mesh = builder.build("soulercoaster", mesh);
            GetComponent<MeshFilter>().mesh = mesh;
        }

        private float getWidth(float time) {
            if (useLineRendererWidthCurve) {
                return lineRendererWidthCurve.Evaluate(time) * width;
            }

            return width;
        }
    }
}