using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEditor;
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
            string url = DreamNetwork.DreamNetworkInstance.GetServerUrl(); 
            var webRequest = new UnityWebRequest(url + $"/{address}", "POST");
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(data.ToString());
            webRequest.uploadHandler = new UploadHandlerRaw(bytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            //Send the request then wait here until it returns
            yield return webRequest.SendWebRequest();
            
            if (webRequest.isNetworkError)
            {
                Debug.Log( webRequest.error + url + $"/{address}");
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
        
        internal static void SetDreamToken(string token)
        {
            StoreData("DTKN", token);
        }
        
        internal static void DeleteDreamToken()
        {
            if (PlayerPrefs.HasKey("DTKN"))
            {
                PlayerPrefs.DeleteKey("DTKN");
                PlayerPrefs.Save();
            }
        }
        
        
        internal static string GetDreamToken()
        {
            string token = DoLoadStoredData("DTKN"); //DTKN = "dream token". wrote it in acronyms for safety
            return token;
        }
        
        private static IEnumerator MakeGetRequest(string address,Action<JToken> onSuccessFull,Action<string> onFail)
        {
            string url = DreamNetwork.DreamNetworkInstance.GetServerUrl();
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
    
    public class DreamByteDisclaimerAttribute : PropertyAttribute
    {
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DreamByteDisclaimerAttribute))]
    public class ShowOnlyDrawer : PropertyDrawer
    {
        #if UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            GUIStyle myStyle = new GUIStyle();

            myStyle.fontSize = 10;
            myStyle.alignment = TextAnchor.UpperLeft;
            myStyle.padding.top = 5;
            myStyle.padding.left = -3;
            myStyle.fontStyle = FontStyle.Bold;
            myStyle.overflow.bottom = 20;
            EditorGUI.DrawRect(new Rect(position.x-11, position.y, position.width+11, position.height + 6), Color.gray);
            Rect r = GUILayoutUtility.GetLastRect();
            EditorGUI.LabelField(r, prop.stringValue, myStyle);
        }
        #endif
    }
    #endif
}

