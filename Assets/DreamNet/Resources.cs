using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DreamNet
{
    public class Resources
    {
        // public static Action<>
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
    }
}