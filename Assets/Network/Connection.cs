using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using NativeWebSocket;
using System.Threading.Tasks;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

namespace MyNetwork
{
    public class Connection : MonoBehaviour
    {
        public WebSocket WebSocket;
        public delegate void ConnectionSuccess();
        public delegate void ReceiveMessageAction(string text);
        public delegate void ReceiveLobbyMessageAction(string text);
        public delegate void ReceiveGameMessageAction(string text);
        public delegate void EnterMemberAction();
        public delegate void ExitMemberAction();
        public delegate void OnMatchMakingSuccessAction();

        public delegate void CompletionFunction();//completion action of send text

        public event ReceiveMessageAction OnReceiveMessage;
        public event EnterMemberAction OnEnterLobby;
        public event ExitMemberAction OnExitLobby;
        public event ReceiveLobbyMessageAction OnReceiveLobbyMessage;
        public event ReceiveGameMessageAction OnReceiveGameMessage;
        public event ConnectionSuccess OnConnectionSuccess;
        public event OnMatchMakingSuccessAction OnMatchMakingSuccess;

        public static Connection Instance {  get; private set; }


        public void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
            DontDestroyOnLoad(this);
        }


        public async Task ConnectToServer()
        {
            try
            {
                WebSocket = new WebSocket("wss://localhost:7004/ws");
                
                WebSocket.OnMessage += (e) =>
                {
                    var message = System.Text.Encoding.UTF8.GetString(e);
                    string request = Encoding.UTF8.GetString(e);
                    JToken jToken = WebSocket.FromBson<JToken>(request);
                    string requestType = (string)jToken["RequestType"];
                    string requestContent = (string)jToken["Content"];
                    Debug.Log(requestType);
                    if (requestType.Equals("LobbyMessage"))
                    {
                        OnReceiveLobbyMessage?.Invoke(requestContent);
                    }
                    if (requestType.Equals("MatchMakingSuccess"))
                    {
                        OnMatchMakingSuccess?.Invoke();
                    }
                };
                WebSocket.OnOpen += () =>
                {
                    if (OnConnectionSuccess != null)
                        OnConnectionSuccess?.Invoke();
                };
                WebSocket.OnClose += (e) =>
                {
                    if (OnExitLobby != null)
                        OnExitLobby?.Invoke();
                };


                await WebSocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void OnWebSocketMessage(byte[] bytes)
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
        }


        
        
        private async void OnApplicationQuit()
        {
            try
            {
                if (WebSocket != null)
                    await WebSocket.Close();
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        public async Task SendText(string text, string request)
        {
            await SendText(text, request, () => { });
        }
        public async Task SendText(string text, string request, CompletionFunction completionFunction)
        {
            try
            {
                if (WebSocket.State == WebSocketState.Open)
                {
                    JObject jObject = new JObject();
                    jObject.Add("Content", text);
                    jObject.Add("RequestType", request);
                    await WebSocket.SendJToken(jObject);
                    completionFunction();
                    //Debug.Log("messageSent");
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }




        
    }
}
