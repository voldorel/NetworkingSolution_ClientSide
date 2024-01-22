using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamNet.Config;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


namespace DreamNet
{
    //[RequireComponent(typeof(DreamNet.Connection))]
    //[RequireComponent(typeof(UnityMainThreadDispatcher))]
    
    [AddComponentMenu("DreamByte/" + nameof(DreamNet.DreamNetwork))]
    [Serializable]
    public class DreamNetwork : MonoBehaviour
    {
        private DreamNetConfig _dreamNetConfig;
        public static DreamNetwork DreamNetworkInstance;
        internal Connection DreamConnection{ get; private set; }
        internal UnityMainThreadDispatcher MainThreadDispatcher { get; private set; }
        private void Awake()
        {
            if (DreamNetwork.DreamNetworkInstance == null)
            {
                DreamConnection = gameObject.AddComponent<Connection>();
                MainThreadDispatcher = gameObject.AddComponent<UnityMainThreadDispatcher>();
                DreamNetworkInstance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            } 
            try
            {
                string assetPath = "Assets/DreamNet/Config/DreamNetConfig.asset";
                DreamNetConfig dreamNetConfig = AssetDatabase.LoadAssetAtPath<DreamNetConfig>(assetPath) as DreamNetConfig;
                _dreamNetConfig = dreamNetConfig;
            }
            catch (Exception e)
            {
                Debug.LogError("Dream config not found.");
                throw;
            }
            
        }

        internal string GetServerAddress()
        {
            return _dreamNetConfig.ServerAddress;
        }
        
        
        
        
        public void SayHello()
        {
            var toolbar = new ToolbarMenu();
            toolbar.text = "NONE";                   //this is displayed as a label on top of VisualElement
 
            //these create options in a dropdown menu embedded in the toolbar and assign functionality to them
        }

        public static void Sasa(DropdownMenuAction dropdownMenuAction)
        {
            Debug.Log("asdasd");
        }
        
    }
}

