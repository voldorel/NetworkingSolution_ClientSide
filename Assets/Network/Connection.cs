﻿using System.Collections;
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
        private string _username;//this should change to an object of a class with its own seprate file in the namespace

        public WebSocket WebSocket;
        #region lobby_actions
        public delegate void ConnectionSuccess();
        public delegate void ConnectionFailure();
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
        public event ConnectionSuccess OnConnectionFailure;
        public event OnMatchMakingSuccessAction OnMatchMakingSuccess;
        #endregion

        #region session_actions
        public delegate void ReceiveSessionText(string text);
        public delegate void MemberEntered();
        public delegate void MemberLeft();
        public delegate void MatchStart();
        public delegate void NetworkFunctionCallDelegate(string text);
        public delegate void MatchEnd();


        public event ReceiveSessionText OnReceiveSessionText;
        public event MemberEntered OnMemberEntered;
        public event MemberLeft OnMemberLeft;
        public event MatchStart OnMatchStart;
        public event NetworkFunctionCallDelegate OnNetworkFunctionCall;
        public event MatchEnd OnMatchEnd;
        #endregion




        public static Connection Instance {  get; private set; }


        public void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
            DontDestroyOnLoad(this);
        }


        public void ConnectToServer(string remoteAddress, string remoteIP)
        {
            try
            {
                WebSocket = new WebSocket("wss://" + remoteAddress + ":" + remoteIP + "/ws");
                
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
                    if (requestType.Equals("LobbyJoined"))
                    {
                        OnEnterLobby?.Invoke();
                    }

                    if (requestType.Equals("SessionStart"))
                    {
                        OnMatchStart?.Invoke();
                    }

                    if (requestType.Equals("SessionEnterMember"))
                    {
                        OnMemberEntered?.Invoke();
                    }

                    if (requestType.Equals("SessionLeaveMember"))
                    {
                        OnMemberLeft?.Invoke();
                    }

                    if (requestType.Equals("SessionText"))
                    {
                        OnReceiveSessionText?.Invoke(requestContent);
                    }

                    if (requestType.Equals("NetworkFunctionCall"))
                    {
                        OnNetworkFunctionCall?.Invoke(requestContent);
                    }

                    if (requestType.Equals("SessionEnd"))
                    {
                        OnMatchEnd?.Invoke();
                    }


                };
                WebSocket.OnOpen += () =>
                {
                    OnConnectionSuccess?.Invoke();
                };
                WebSocket.OnError += (e) =>
                {
                    OnConnectionFailure?.Invoke();
                };
                WebSocket.OnClose += (e) =>
                {
                    if (OnExitLobby != null)
                        OnExitLobby?.Invoke();
                };


                WebSocket.Connect();
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

        public string GetUsername()
        {
            return _username;
        }

        public void SetUsername(string username)
        {
            _username = username;
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
                    completionFunction?.Invoke();
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
