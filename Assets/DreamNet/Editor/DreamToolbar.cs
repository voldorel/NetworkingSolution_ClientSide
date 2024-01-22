using System.Collections;
using System.Collections.Generic;
using DreamNet.Config;
using UnityEditor;
using UnityEngine;

namespace DreamNet.Editor
{
    
    [InitializeOnLoad]
    public class DreamToolbar
    {
        
        static DreamToolbar()
        {
            EditorApplication.delayCall += OnDelayCall;
        }

        static void OnDelayCall()
        {
            EditorApplication.delayCall -= OnDelayCall;

            // Add your menu item to the toolbar
            //EditorApplication.ExecuteMenuItem("DreamNet/Config");
        }

        [MenuItem("DreamNet/Config")]
        public static void ShowCustomToolbar()
        {

            // Replace YourScriptableObjectPath with the actual path to your ScriptableObject
            string assetPath = "Assets/DreamNet/Config/DreamNetConfig.asset";
        
            // Load the ScriptableObject
            var scriptableObject = AssetDatabase.LoadAssetAtPath<DreamNetConfig>(assetPath);

            if (scriptableObject != null)
            {
                // Select and show the ScriptableObject in the Inspector
                Selection.activeObject = scriptableObject;
                EditorGUIUtility.PingObject(scriptableObject.GetInstanceID());
            }
            else
            {
                Debug.LogError("DreamNet config file not found");
            }
        }
        
        
    }
}
