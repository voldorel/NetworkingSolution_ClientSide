using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamNet.Config;
using DreamNet.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Object = System.Object;


namespace DreamNet
{
    //[RequireComponent(typeof(DreamNet.Connection))]
    //[RequireComponent(typeof(UnityMainThreadDispatcher))]
    
    [AddComponentMenu("DreamByte/" + nameof(DreamNet.DreamNetwork))]
    [Serializable]
    public class DreamNetwork : MonoBehaviour
    {

        [DreamByteDisclaimer] public string CopyWrite = "DreamNet is developed by Mehdi Parvan & Taha Gharanfoli";
        [DreamByteDisclaimer] public string CopyWrite1 = "*important* Simply include DreamNet in your game and Call";
        [DreamByteDisclaimer] public string CopyWrite2 = "StartDreamServer() from the singleton instance";
        [DreamByteDisclaimer] public string CopyWrite3 = "in order to initiate the connection";
        [DreamByteDisclaimer] public string CopyWrite4 = "\n";
        [DreamByteDisclaimer] public string CopyWrite5 = "DreamByte© 2024";
        
        
        [HideInInspector] public DreamNetConfig _dreamNetConfig;
        public static DreamNetwork DreamNetworkInstance;
        
        internal Connection DreamConnection{ get; private set; }
        internal UnityMainThreadDispatcher MainThreadDispatcher { get; private set; }
        
        
        
        public static MetaData UserMetaData;
        public static ProfileMetaData ProfileMetaData;
        
        
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
#if UNITY_EDITOR
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
#endif
        }

        internal string GetServerUrl()
        {
#if UNITY_EDITOR
            string assetPath = "Assets/DreamNet/Config/DreamNetConfig.asset";
            DreamNetConfig dreamNetConfig = AssetDatabase.LoadAssetAtPath<DreamNetConfig>(assetPath) as DreamNetConfig;
#endif
            return "http://" + _dreamNetConfig.ServerAddress;
        }
        
        internal string GetServerAddress()
        {
            return _dreamNetConfig.ServerAddress;
        }

        public void StartDreamServer()
        {
            DreamConnection.StartDreamNet();
        }

        public int GetRandomSeed()
        {
            return DreamConnection.GetRandomeSeed();
        }

        private void OnEnable()
        {
            // edit by taha -------------------------------------
            ProfileMetaData = new ProfileMetaData();
            UserMetaData = new MetaData("Root",null);
            UserMetaData.OnSetData += PushMetaData;
            // --------------------------------------------------
        }

        private void OnDestroy()
        {
            UserMetaData.OnSetData -= PushMetaData;
        }
        public void PushMetaData(MetaData metaData)
        {
            Debug.Log("Pushed Successfully !!!");
            JObject data = new JObject();
            JArray jArray = new JArray(metaData.ModifiedAddress());
            data.Add("Address",jArray);
            data.Add("Value",metaData.NodeValue.ToString());
            data.Add("DataType",metaData.ValueDataType.ToString());
            Connection.Instance.SendText(JsonConvert.SerializeObject(data), "ChangeUserMetaData", () =>
            {
                Debug.Log("Pushed Successfully !!!");
                metaData.OnUpdate?.Invoke(metaData);
            });
        }

        public void InitUserMetaData(string value)
        {
            var jsonDict = JObject.Parse(value);
            FillMetaData(UserMetaData, jsonDict);
        }

        public void InitProfileMetaData(string nickName, string profileInfo)
        {
            ProfileMetaData.Init(nickName,profileInfo);
        }
        private void FillMetaData(MetaData metaData, JObject jObject)
        {
            foreach (var item in jObject)
            {
                var key = item.Key;
                var value = item.Value;
                if (value is JObject)
                {
                    MetaData childMetaData = new MetaData(key, metaData);
                    FillMetaData(childMetaData,value as JObject);
                    metaData[key] = childMetaData;
                }
                else
                {
                    metaData[key]= new MetaData(key,metaData)
                    {
                        NodeValue = value
                    };
                }
            }
        }
    }
}

