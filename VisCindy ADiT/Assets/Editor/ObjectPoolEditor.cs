using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(ObjectPool))]
    public class ObjectPoolEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw the default inspector

            ObjectPool script = (ObjectPool)target;

            if (GUILayout.Button("Pre-populate Pools"))
            {
                script.CreatePool(script.nodePrefab, script.initialNodes, "Nodes");
                script.CreatePool(script.linePrefab, script.initialLines, "Lines");

                // Force Unity to serialize the changes
                EditorUtility.SetDirty(script); 
                serializedObject.ApplyModifiedProperties(); 
            }
        }
    }
}