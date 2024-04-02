using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DreamNet
{
    public class Resources
    {
        // public static Action<>
        private static Dictionary<string, long> _resourceMap = new Dictionary<string, long>();
        public static void EarnResources(string resourceType, long value, string description="")
        {
            JObject data = new JObject();
            data.Add("ResourceType",resourceType);
            data.Add("Value",value);
            data.Add("Description",description);
            Connection.Instance.SendText(JsonConvert.SerializeObject(data), "UpdateResourceRequest", () =>
            {
                Debug.Log("Earned Successfully !!!");
            });
        }
        public static long GetResourceValue(string resourceType)
        {
            if (_resourceMap.ContainsKey(resourceType)) return _resourceMap[resourceType];
            return 0;
        }
        public static void OnChangeResource(string data)
        {
            Debug.Log(data);
            JObject tempData = JObject.Parse(data);

            foreach (var item in tempData)
            {
                if (_resourceMap.ContainsKey(item.Key))
                {
                    _resourceMap[item.Key] = (long)item.Value;
                }
                else
                {
                    _resourceMap.Add(item.Key, (long)item.Value);
                }
                Debug.Log("value :  "+(long)item.Value);
            }
          
        }
        public static void Init(string data)
        {
            data=data.Replace("NumberLong(", "").Replace(")", "");
            JObject jsonData=JObject.Parse(data);
            // Debug.Log("Iniiiiiiiiiiiiiiii   "+resourceData.ToString());
            _resourceMap.Clear();
            foreach (var item in jsonData)
            {
                _resourceMap.Add(item.Key,long.Parse(item.Value.ToString()));
            }
        }
        
    }
}