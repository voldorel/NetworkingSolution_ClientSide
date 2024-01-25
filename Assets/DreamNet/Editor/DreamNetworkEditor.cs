using UnityEditor;
using UnityEngine;


namespace DreamNet.Editor
{
    [CustomEditor(typeof(DreamNetwork))]
    public class DreamNetworkEditor : UnityEditor.Editor
    {
        private static readonly string[] _dontIncludeMe = new string[]{"m_Script", "m_PropertiesHash"};
    
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, _dontIncludeMe);

            serializedObject.ApplyModifiedProperties();
        }
        
    }
    
}
