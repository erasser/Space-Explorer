using soulercoaster.scripts;
using soulercoasterLite.scripts;
using UnityEditor;
using UnityEngine;

namespace soulercoaster.Editor {
    public class PreventMeshSaving : AssetModificationProcessor {
        private static string[] OnWillSaveAssets(string[] paths) {
            var coasters = Object.FindObjectsOfType<SoulerCoaster>();
            foreach (var coaster in coasters) {
                if (coaster.preventMeshSaving) {
                    coaster.GetComponent<MeshFilter>().mesh = null;
                }
            }

            EditorApplication.update += RefreshScene;

            return paths;
        }

        private static void RefreshScene() {
            EditorApplication.update -= RefreshScene;
            var coasters = Object.FindObjectsOfType<SoulerCoaster>();
            foreach (var coaster in coasters) {
                if (coaster.preventMeshSaving) {
                    coaster.GetComponent<MeshFilter>().mesh = coaster.getMesh();
                }
            }
        }
    }
}