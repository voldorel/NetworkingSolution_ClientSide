using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using NativeWebSocket;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

namespace MyNetwork
{
    public sealed class Connection : MonoBehaviour
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
        public delegate void MatchMakingSuccessAction();
        public delegate void LoginSuccessAction(string text);

        public delegate void CompletionFunction();//completion action of send text

        public event ReceiveMessageAction OnReceiveMessage;
        public event EnterMemberAction OnEnterLobby;
        public event ExitMemberAction OnExitLobby;
        public event ReceiveLobbyMessageAction OnReceiveLobbyMessage;
        public event ReceiveGameMessageAction OnReceiveGameMessage;
        public event ConnectionSuccess OnConnectionSuccess;
        public event ConnectionSuccess OnConnectionFailure;
        public event MatchMakingSuccessAction OnMatchMakingSuccess;
        public event LoginSuccessAction OnLoginSuccessAction;
        #endregion

        #region session_actions
        public delegate void ReceiveSessionText(string text);
        public delegate void MemberEntered();
        public delegate void MemberLeft();
        public delegate void MatchStart(string text);
        public delegate void NetworkFunctionCallDelegate(string text);
        public delegate void MatchEnd();
        public delegate void SessionOutOfSync();
        public delegate void SessionSynchronizationDone();


        public event ReceiveSessionText OnReceiveSessionText;
        public event MemberEntered OnMemberEntered;
        public event MemberLeft OnMemberLeft;
        public event MatchStart OnMatchStart;
        public event NetworkFunctionCallDelegate OnNetworkFunctionCall;
        public event MatchEnd OnMatchEnd;
        public event SessionOutOfSync OnSessionOutOfSync;
        public event SessionSynchronizationDone OnSessionSyncFinish;
        #endregion


        #region variables
        private int _sessionTime;
        private int _clientTime;
        private double _tickrateFixedTime;
        private bool _missingNetEventsDownloaded;
        internal bool IsLoadingGameSession { get; private set; }
        private static readonly ConcurrentQueue<NetEvent> _netCallPreSyncQueue = new ConcurrentQueue<NetEvent>();
        private static readonly ConcurrentQueue<NetEvent> _netCallQueue = new ConcurrentQueue<NetEvent>();
        private class NetEvent
        {
            private Action _netCallAction;
            private int _netcallTime;
            public NetEvent(Action netCallAction, int netcallTime)
            {
                _netCallAction = netCallAction;
                _netcallTime = netcallTime;
            }
            public Action GetNetCallAction ()
            {
                return _netCallAction;
            }
            public int GetNetcallTime()
            {
                return _netcallTime;
            }
        }
        #endregion



        internal static Connection Instance {  get; private set; }


        public void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
            DontDestroyOnLoad(this);
            ResetSessionConfig();
            IsLoadingGameSession = false;
            _missingNetEventsDownloaded = false;
            _tickrateFixedTime = 50d;
        }


        private void ResetSessionConfig()
        {
            _sessionTime = 0;
            _clientTime = 0;
            _missingNetEventsDownloaded = false;
        }


        public void ConnectToServer(string remoteAddress, string remoteIP)
        {
            try
            {
                WebSocket = new WebSocket("ws://" + remoteAddress + ":" + remoteIP + "/ws");
                
                WebSocket.OnMessage += (e) =>
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
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
                            ResetSessionConfig();
                            OnMatchMakingSuccess?.Invoke();
                        }
                        if (requestType.Equals("LobbyJoined"))
                        {
                            OnEnterLobby?.Invoke();
                        }

                        if (requestType.Equals("SessionStart"))
                        {
                            OnMatchStart?.Invoke(requestContent);
                        }

                        if (requestType.Equals("SessionEnterMember"))
                        {
                            OnMemberEntered?.Invoke();
                        }

                        if (requestType.Equals("LoginSuccess"))
                        {
                            ResetSessionConfig();
                            JObject keyValuePairs = JObject.Parse(requestContent);
                            _tickrateFixedTime = (double)keyValuePairs["ServerTickrateFixedTime"];
                            OnLoginSuccessAction?.Invoke(requestContent);
                        }



                        if (requestType.Equals("SessionText"))
                        {
                            OnReceiveSessionText?.Invoke(requestContent);
                        }

                        if (requestType.Equals("NetworkFunctionCall"))
                        {
                            try
                            {
                                lock (_netCallQueue)
                                {
                                    NetEvent netEvent = new NetEvent(() => {
                                        JToken netCallToken = WebSocket.FromBson<JToken>(requestContent);
                                        OnNetworkFunctionCall?.Invoke((string)netCallToken["Content"]);
                                    }, _sessionTime);
                                    _netCallQueue.Enqueue(netEvent);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }

                        if (requestType.Equals("PreSyncNetworkFunctionCall"))
                        {
                            try
                            {
                                lock (_netCallPreSyncQueue)
                                {
                                    NetEvent netEvent = new NetEvent(() => {
                                        JObject keyValuePairs = JObject.Parse(requestContent);
                                        int eventTime = (int)keyValuePairs["eventTime"];
                                        string eventBody = (string)keyValuePairs["eventBody"];
                                        JToken netCallToken = WebSocket.FromBson<JToken>(eventBody);
                                        OnNetworkFunctionCall?.Invoke((string)netCallToken["Content"]);
                                    }, _sessionTime);
                                    _netCallPreSyncQueue.Enqueue(netEvent);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }

                        if (requestType.Equals("SessionEnd"))
                        {
                            OnMatchEnd?.Invoke();
                        }


                        if (requestType.Equals("SyncTransferFinished"))
                        {
                            _missingNetEventsDownloaded = true;
                        }

                        if (requestType.Equals("GameData"))
                        {
                        }


                        if (requestType.Equals("GameSynchronize"))//list of all netcalls and on going events in current game session
                        {
                        }
                    });
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
                WebSocket.OnMessageBinary += e =>
                {
                    //currently only used to keep track of session time
                    Debug.Log(e + " " + _sessionTime + " " + _clientTime);
                    if (e - _sessionTime > 1 && IsLoadingGameSession == false)
                    {
                        IsLoadingGameSession = true;
                        UnityMainThreadDispatcher.Instance()?.Enqueue(() => {
                            OnSessionOutOfSync?.Invoke();
                            StartCoroutine(SynchronizeCoroutine());

                            Task.Run(async () =>
                            {
                                JObject keyValuePairs = new JObject();
                                keyValuePairs.Add("startingTime", _clientTime);
                                keyValuePairs.Add("endingTime", _sessionTime);
                                await SendText(keyValuePairs.ToString(), "SynchronizaionRequest");
                            });
                        });
                        //needs to request a sync and in the result, set IsLoadingGameSession to false and _sessionTime <= e
                    }
                    else
                    {
                        _sessionTime = e;

                        _clientTime++;
                        
                    }
                };


                WebSocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }



        public void Update()
        {
            try
            {
                if (!IsLoadingGameSession)
                {
                    while (_netCallQueue.Count > 0)
                    {
                        NetEvent netEvent;
                        _netCallQueue.TryDequeue(out netEvent);
                        UnityMainThreadDispatcher.Instance().Enqueue(netEvent.GetNetCallAction());
                    }
                }
            } catch (Exception ex)
            {
                Debug.LogError("Critical error when syncing game " + ex);
            }
        }

        internal double GetTickrateFixedTime()
        {
            return _tickrateFixedTime;
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




        public int GetSessionTime()
        {
            return _sessionTime;
        }



        private IEnumerator ProcessNetCallQueue(ConcurrentQueue<NetEvent> netEventsQueue)
        {
            float fixedTimePeriod = (float)_tickrateFixedTime;
            while (netEventsQueue.Count > 0)
            {
                NetEvent netEvent;
                netEventsQueue.TryDequeue(out netEvent);
                int netcallTime = netEvent.GetNetcallTime();
                if (netcallTime > _clientTime)
                {
                    yield return new WaitForSeconds(fixedTimePeriod * (netcallTime - _clientTime));
                    _clientTime += (netcallTime - _clientTime);
                }
                UnityMainThreadDispatcher.Instance().Enqueue(netEvent.GetNetCallAction());
            }
        }




        private IEnumerator SynchronizeCoroutine()
        {

            //ask for all netcalls and wait for the list to fill
            //after that, start processing list 
            //needs to know the tickrate for the timer to know how long to wait in seconds 
            //also need to set timescale to server delta time at the start of the game
            var prevTime = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitUntil(() => _missingNetEventsDownloaded);
            
            //get list of events from server


            Time.timeScale = 100;
            //start synching while listerning to new events
            yield return ProcessNetCallQueue(_netCallPreSyncQueue);
            //yield return new WaitForSeconds(1double / server tick rate); // this value meaning 1 / tickrate should come from the srver itself
            //done. time is set to entrance time which is lower than current time.
            //now that we're cought up with the previous events let's start catching up to current events
            yield return ProcessNetCallQueue(_netCallQueue);
            //while
            //done. enghadr bayad sari poshte timere felie server bodoe le belakhare time ha yeki beshe va liste net call jadida khali besehe
            Debug.Log((float)_tickrateFixedTime);
            /*while (true)
            {
                if (_clientTime <= _sessionTime)
                    break;
                yield return new WaitForSeconds((float)_tickrateFixedTime);
                _clientTime++;
            }*/
            Time.timeScale = prevTime;
            _clientTime = _sessionTime;
            Debug.Log("WE MADE IT" + _sessionTime + " " + _clientTime);
            IsLoadingGameSession = false;
            _missingNetEventsDownloaded = false;
            OnSessionSyncFinish?.Invoke();
        }
    }
}
