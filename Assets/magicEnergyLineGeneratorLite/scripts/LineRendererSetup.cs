#if UNITY_EDITOR
using UnityEditor;
#endif 
using UnityEngine;

namespace soulercoasterLite.scripts {
    public static class LineRendererSetup {
        
        public static void setupLineRenderer(this MonoBehaviour monoBehaviour) {
            if (monoBehaviour.GetComponent<LineRenderer>().sharedMaterial != null) {
                return;
            }

            string[] guids2 = AssetDatabase.FindAssets("defaultLineMaterial t:material");
            if (guids2.Length == 0) {
                return;
            }

            monoBehaviour.GetComponent<LineRenderer>().sharedMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids2[0]));
            monoBehaviour.GetComponent<LineRenderer>().useWorldSpace = false;
        }
    }
}