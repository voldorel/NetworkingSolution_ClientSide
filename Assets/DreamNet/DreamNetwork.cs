using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamNet.Config;
using UnityEditor;


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
                string[] matchingAssets = AssetDatabase.FindAssets("DreamNetConfig", new string[] { "Assets/DreamNet/Config" });
                foreach (var asset in matchingAssets)
                {
                    var configPath = AssetDatabase.GUIDToAssetPath(asset);
                    DreamNetConfig dreamConfig = AssetDatabase.LoadAssetAtPath(configPath, typeof(DreamNetConfig)) as DreamNetConfig;
                    _dreamNetConfig = dreamConfig;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Dream config not found.");
                throw;
            }
            
        }
    }
}

