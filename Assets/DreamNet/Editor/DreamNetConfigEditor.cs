using System.Collections;
using System.Collections.Generic;
using DreamNet.Config;
using UnityEditor;
using UnityEngine;

namespace DreamNet.Editor
{
    
    [CustomEditor(typeof(DreamNetConfig))]
    public class DreamNetConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DreamNetConfig myScript = (DreamNetConfig)target;
            myScript.DoRefreshLoginTokenText();
            DrawDefaultInspector();

            if (GUILayout.Button("Clear login token"))
            {
                myScript.DoClearLoginToken();
            }
            
        }
    }
}
