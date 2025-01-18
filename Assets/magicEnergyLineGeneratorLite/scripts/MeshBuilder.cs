using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace soulercoasterLite.scripts {
    public class MeshBuilder {
        public List<Vector3> vectorList = new();
        private Vector3[] vertices = Array.Empty<Vector3>();
        public List<int> triangleList = new();
        private int[] triangles = Array.Empty<int>();
        public List<Vector4> tangentList = new();
        private Vector4[] tangents = Array.Empty<Vector4>();
        public List<Vector3> normalList = new();
        private Vector3[] normals = Array.Empty<Vector3>();
        public List<Vector2> uvCoordinates1 = new();
        private Vector2[] uv1 = Array.Empty<Vector2>();
        public List<Vector2> uvCoordinates2 = new();
        private Vector2[] uv2 = Array.Empty<Vector2>();
        public int i;

        public Mesh build(string name, [CanBeNull] Mesh existing = null) {
            var indexFormat = IndexFormat.UInt16;
            if (vectorList.Count > 65534) {
                indexFormat = IndexFormat.UInt32;
            }

            var mesh = existing;
            if (mesh == null) {
                mesh = new Mesh();
            }

            mesh.name = name;

            mesh.indexFormat = indexFormat;

            if (vertices.Length != vectorList.Count) {
                vertices = new Vector3[vectorList.Count];
                tangents = new Vector4[vectorList.Count];
                normals = new Vector3[vectorList.Count];
                uv1 = new Vector2[vectorList.Count];
                uv2 = new Vector2[vectorList.Count];
            }

            vectorList.CopyTo(vertices);

            if (mesh.vertexCount > vertices.Length) {
                mesh.SetTriangles(Array.Empty<int>(), 0);
            }

            mesh.SetVertices(vertices);

            tangentList.CopyTo(tangents);
            mesh.SetTangents(tangents);

            normalList.CopyTo(normals);
            mesh.SetNormals(normals);

            uvCoordinates1.CopyTo(uv1);
            mesh.SetUVs(0, uv1);
            uvCoordinates2.CopyTo(uv2);
            mesh.SetUVs(1, uv2);

            if (triangles.Length != triangleList.Count) {
                triangles = new int[triangleList.Count];
            }

            triangleList.CopyTo(triangles);
            mesh.SetTriangles(triangles, 0);

            mesh.RecalculateBounds();

            return mesh;
        }

        public void reset(bool resetOnlyVertices) {
            i = 0;
            vectorList.Clear();
            tangentList.Clear();
            normalList.Clear();
            if (resetOnlyVertices) {
                return;
            }

            triangleList.Clear();
            uvCoordinates1.Clear();
            uvCoordinates2.Clear();
        }
    }
}