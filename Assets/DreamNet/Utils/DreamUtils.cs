using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace DreamNet.Utils
{

    internal static class DreamUtils
    {
        public static void StoreData(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        public static string DoLoadStoredData(string key)
        {
            string value = PlayerPrefs.GetString(key, "NONE");
            return value;
        }
        
        public static IEnumerator Register(Action<JToken> onSuccess, Action<string> onFail)
        {
            JObject json = new JObject();
            json.Add("DeviceId", SystemInfo.deviceUniqueIdentifier);
            json.Add("OperatingSystem", SystemInfo.operatingSystem);
            return MakePostRequest("authentication/register", json, onSuccess, onFail);
        }
        
        private static IEnumerator MakePostRequest(string address, JObject data, Action<JToken> onSuccessFull,
            Action<string> onFail)
        {
            string url = DreamNetwork.GetServerUrl(); 
            var webRequest = new UnityWebRequest(url + $"/{address}", "POST");
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(data.ToString());
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            //Send the request then wait here until it returns
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error While Sending: " + webRequest.error + url + $"/{address}");
                onFail?.Invoke(webRequest.error);
            }
            else
            {
                //Debug.Log("Received: " + webRequest.downloadHandler.text);
                JObject resultJObject= JObject.Parse(webRequest.downloadHandler.text);
                Result result = new Result(int.Parse( resultJObject["status"].ToString()), resultJObject["value"]);
                if(result.Status == ResultStatus.Success) onSuccessFull?.Invoke(result.Value);
                else onFail?.Invoke(result.Value.ToString());

            }
        }
        private static IEnumerator MakeGetRequest(string address,Action<JToken> onSuccessFull,Action<string> onFail)
        {
            string url = DreamNetwork.GetServerUrl();
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url + $"/{address}"))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Error While Sending: " + webRequest.error);
                    onFail?.Invoke(webRequest.error);
                }
                else
                {
                    Debug.Log("Received: " + webRequest.downloadHandler.text);
                    JObject resultJObject= JObject.Parse(webRequest.downloadHandler.text);
                    Result result = new Result(int.Parse( resultJObject["status"].ToString()), resultJObject["value"]);
                    if(result.Status == ResultStatus.Success) onSuccessFull?.Invoke(result.Value);
                    else onFail?.Invoke(result.Value["error"].ToString());
                }
            }
        }
    }

    public enum ResultStatus
    {
        Error=1,
        Success=0 // was 1 and error was 1. Dev Taha should provide explaination
    }
    public class Result
    {
        public ResultStatus Status;
        public JToken Value;
        public Result(int status, JToken value)
        {
            Status = (ResultStatus)status;
            Value =  value;
        }
    }
}

