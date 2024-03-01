using System.Collections;
using UnityEngine;
using System.Text;
using NativeWebSocket;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using DreamNet.Utils;
using Newtonsoft.Json.Linq;

namespace DreamNet
{
    public sealed class Connection : MonoBehaviour
    {
        private string _userId;//this should change to an object of a class with its own seprate file in the namespace

        private int _currentRandomSeed; // needs to be moved to gamesession with proper implementation
        
        public WebSocket WebSocket;
        #region lobby_actions
        public delegate void InitialLoading();
        public delegate void ConnectionSuccess();
        public delegate void ConnectionFailure();
        public delegate void ReceiveMessageAction(string text);
        public delegate void ReceiveLobbyMessageAction(string tex);
        public delegate void ReceiveGameMessageAction(string text);
        public delegate void ExitLobbyAction();
        public delegate void EnterLobbyAction();
        public delegate void MatchMakingSuccessAction(string matchData);
        public delegate void LoginSuccessAction(string text);

        public delegate void UpdateResources(string text);

        public delegate void CompletionFunction();//completion action of send text

        public event InitialLoading OnInitialLoading;
        public event ReceiveMessageAction OnReceiveMessage;
        public event EnterLobbyAction OnEnterLobby;
        public event ExitLobbyAction OnExitLobby;
        public event ReceiveLobbyMessageAction OnReceiveLobbyMessage;
        public event ReceiveGameMessageAction OnReceiveGameMessage;
        public event ConnectionSuccess OnConnectionSuccess;
        public event ConnectionFailure OnConnectionFailure;
        public event MatchMakingSuccessAction OnMatchMakingSuccess;
        public event LoginSuccessAction OnLoginSuccessAction;
        public event UpdateResources OnUpdateResouresAction;
        #endregion

        #region session_actions
        public delegate void ReceiveSessionText(string text);
        public delegate void MatchStart(string text);
        public delegate void NetworkFunctionCallDelegate(string text);
        public delegate void MatchEnd();
        public delegate void SessionOutOfSync();
        public delegate void SessionSynchronizationDone();
        public delegate void EnterPlayerAction(string text);
        public delegate void ExitPlayerAction(string text);

        public delegate void SessionDisconnected(); // TODO
        public delegate void SessionConnected(); // TODO

        public delegate void NetworkUpdateAction();


        public event ReceiveSessionText OnReceiveSessionText;
        public event MatchStart OnMatchStart;
        public event NetworkFunctionCallDelegate OnNetworkFunctionCall;
        public event MatchEnd OnMatchEnd;
        public event SessionOutOfSync OnSessionOutOfSync;
        public event SessionSynchronizationDone OnSessionSyncFinish;
        public event NetworkUpdateAction OnNetworkUpdate;
        public event EnterPlayerAction OnPlayerEnterSession;
        public event ExitPlayerAction OnPlayerExitSession;

        #endregion


        #region variables
        private int _sessionTime;
        private int _clientTime;
        private double _tickrateFixedTime;
        private bool _missingNetEventsDownloaded;
        private bool _isGameSceneLoaded; // set to true every time the game scene starts. bc we wanna wait for the scene to load before requesting a synchronization
        public bool IsLoadingGameSession { get; private set; }
        private static readonly ConcurrentQueue<NetEvent> _netCallPreSyncQueue = new ConcurrentQueue<NetEvent>();
        private static readonly ConcurrentQueue<NetEvent> _liveEventQueue = new ConcurrentQueue<NetEvent>();
        #endregion
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
            _isGameSceneLoaded = false;
            _tickrateFixedTime = 50d;
        }

        internal void SetRandomSeed(int randomSeed)
        {
            _currentRandomSeed = randomSeed;
            UnityEngine.Random.InitState(randomSeed);
            //Debug.Log("Random seed is set : " + randomSeed);
        }

        internal int GetRandomeSeed()
        {
            return _currentRandomSeed;
        }
        
        private void ResetSessionConfig()
        {
            _sessionTime = 0;
            _clientTime = 0;
            _missingNetEventsDownloaded = false;
        }

        public async void DoLogin(string newUsername)
        {
            try
            {
                await Connection.Instance.SendText(newUsername, "UserLogin");
            }
            catch
            {
                Debug.LogError("Login Failed");
            }
        }
        internal void ResetGameSyncState()
        {
            _isGameSceneLoaded = false;
        }

        internal void DoReadySyncState()
        {
            StartCoroutine(WaitBeforeResync());
        }

        private IEnumerator WaitBeforeResync()
        {
            yield return new WaitForFixedUpdate();
            _isGameSceneLoaded = true;
        }
        
        
        private IEnumerator TrySignIn()
        {
            string token = DreamUtils.GetDreamToken();
            if (token.Equals("NONE"))
            {
                yield return DreamUtils.Register((e) =>
                {
                    try
                    {
                        token = (string)e["token"];
                        DreamUtils.SetDreamToken((string)e["token"]);
                        
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        OnConnectionFailure?.Invoke();
                    }
                    JObject jsonData = JObject.Parse(e.ToString());
                    
                }, (e) =>
                {
                    OnConnectionFailure?.Invoke();
                    //Debug.LogError(e);
                });
                
            }
            //DoLogin(token);
            DoConnectToServer(DreamNetwork.DreamNetworkInstance.GetServerAddress(), token);
            //DoConnectToServer("localhost:5000", token);
        }
        
        /// <summary>
        /// Called at startup of the game to initialize DreamNet
        /// and load player data through websocket connection
        ///---------------------------------------------------
        /// Authors: Mehdi Parvan - Taha Gharanfoli
        /// </summary>
        public void StartDreamNet()
        {
            try
            {
                OnInitialLoading?.Invoke();
                StartCoroutine(TrySignIn());
            }
            catch
            {
                OnConnectionFailure?.Invoke();
            }
        }

        public void DoConnectToServer(string remoteAddress, string token)
        {
            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers["Authorization"] = token;
                WebSocket = new WebSocket("ws://" + remoteAddress + "/ws", headers);
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
                            OnMatchMakingSuccess?.Invoke(requestContent);
                        }
                        if (requestType.Equals("LobbyJoined"))
                        {
                            OnEnterLobby?.Invoke();
                        }

                        /*if (requestType.Equals("SessionStart"))
                        {
                            OnMatchStart?.Invoke(requestContent);
                        }*/



                        if (requestType.Equals("PlayerLeftSession"))
                        {
                            lock (_liveEventQueue)
                            {
                                NetEvent netEvent = new NetEvent(() => {
                                    OnPlayerExitSession?.Invoke(requestContent);
                                }, _sessionTime);
                                _liveEventQueue.Enqueue(netEvent);
                            }
                        }

                        if (requestType.Equals("PlayerEnteredSession"))
                        {
                            lock (_liveEventQueue)
                            {
                                NetEvent netEvent = new NetEvent(() => {
                                    OnPlayerEnterSession?.Invoke(requestContent);
                                }, _sessionTime);
                                _liveEventQueue.Enqueue(netEvent);
                            }
                        }

                        if (requestType.Equals("LoginSuccess"))
                        {
                            ResetSessionConfig();
                            JObject keyValuePairs = JObject.Parse(requestContent);
                            _tickrateFixedTime = (double)keyValuePairs["ServerTickrateFixedTime"];
                            OnLoginSuccessAction?.Invoke(requestContent);
                        }
                        //todo check later ###Taha 
                        if (requestType.Equals("UpdateResourceRequest"))
                        {
                            OnUpdateResouresAction?.Invoke(requestContent);
                        }
                        if (requestType.Equals("SessionText"))
                        {
                            OnReceiveSessionText?.Invoke(requestContent);
                        }

                        if (requestType.Equals("NetworkFunctionCall"))
                        {
                            try
                            {
                                int sessionTime = _sessionTime;
                                lock (_liveEventQueue)
                                {
                                    NetEvent netEvent = new NetEvent(() => {
                                        JToken netCallToken = WebSocket.FromBson<JToken>(requestContent);
                                        OnNetworkFunctionCall?.Invoke((string)netCallToken["Content"]);
                                    }, sessionTime);
                                    _liveEventQueue.Enqueue(netEvent);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }

                        if (requestType.StartsWith("PreSync")) {
                            try
                            {
                                JObject keyValuePairs = JObject.Parse(requestContent);
                                int eventTime = (int)keyValuePairs["eventTime"];
                                if (requestType.Equals("PreSyncNetworkFunctionCall"))
                                {
                                    lock (_netCallPreSyncQueue)
                                    {
                                        NetEvent netEvent = new NetEvent(() => {
                                            string eventBody = (string)keyValuePairs["eventBody"];
                                            JToken netCallToken = WebSocket.FromBson<JToken>(eventBody);
                                            OnNetworkFunctionCall?.Invoke((string)netCallToken["Content"]);
                                        }, eventTime);
                                        _netCallPreSyncQueue.Enqueue(netEvent);
                                    }
                                } else if (requestType.Equals("PreSyncPlayerEntered"))
                                {
                                    lock (_netCallPreSyncQueue)
                                    {
                                        NetEvent netEvent = new NetEvent(() => {
                                            string eventBodyContent = GetContentFromBody((string)keyValuePairs["eventBody"]);
                                            OnPlayerEnterSession?.Invoke(eventBodyContent);
                                        }, eventTime);
                                        _netCallPreSyncQueue.Enqueue(netEvent);
                                    }
                                }
                                else if (requestType.Equals("PreSyncPlayerLeft"))
                                {
                                    lock (_netCallPreSyncQueue)
                                    {
                                        NetEvent netEvent = new NetEvent(() => {
                                            string eventBodyContent = GetContentFromBody((string)keyValuePairs["eventBody"]);
                                            OnPlayerExitSession?.Invoke(eventBodyContent);
                                        }, eventTime);
                                        _netCallPreSyncQueue.Enqueue(netEvent);
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }

                        if (requestType.Equals("SessionEnd"))
                        {
                            _isGameSceneLoaded = false; // no need to keep syncing because game has already been finished
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
                    //Debug.Log(e + " " + _sessionTime + " " + _clientTime);
                    if (!_isGameSceneLoaded)
                        return;
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
                                //keyValuePairs.Add("endingTime", _sessionTime); changed this to e at Jan 2024
                                keyValuePairs.Add("endingTime", e);
                                await SendText(keyValuePairs.ToString(), "SynchronizaionRequest");
                            });
                        });
                        //needs to request a sync and in the result, set IsLoadingGameSession to false and _sessionTime <= e
                    }
                    else
                    {
                        _sessionTime = e;


                        _clientTime++;
                        OnNetworkUpdate?.Invoke();
                    }
                };


                WebSocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private async void OnApplicationQuit ()
        {
            if (WebSocket != null)
            {
                if (WebSocket.State == WebSocketState.Open)
                    await WebSocket.Close();
            }
        }

        private async void OnDestroy()
        {
            if (WebSocket != null)
            {
                if (WebSocket.State == WebSocketState.Open)
                    await WebSocket.Close();
            }
        }


        public void Update()
        {
            try
            {
                if (!IsLoadingGameSession)
                {
                    while (_liveEventQueue.Count > 0)
                    {
                        NetEvent netEvent;
                        _liveEventQueue.TryDequeue(out netEvent);
                        UnityMainThreadDispatcher.Instance().Enqueue(netEvent.GetNetCallAction());
                    }
                }
            } catch (Exception ex)
            {
                Debug.LogError("Critical error when syncing game " + ex);
            }
        }

        private string GetContentFromBody(string eventBody)
        {
            JObject eventBodyJToken = JObject.Parse(eventBody);
            string eventBodyContent = (string)eventBodyJToken["Content"];
            return eventBodyContent;
        }

        internal double GetTickrateFixedTime()
        {
            return _tickrateFixedTime;
        }

        public void OnWebSocketMessage(byte[] bytes)
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
        }

        public string GetUserId()
        {
            return _userId;
        }

        public void SetUserId(string userId)
        {
            _userId = userId;
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



        private IEnumerator ProcessEventQueue(ConcurrentQueue<NetEvent> netEventsQueue)
        {
            float fixedTimePeriod = (float)_tickrateFixedTime;
            while (netEventsQueue.Count > 0)
            {
                NetEvent netEvent;
                netEventsQueue.TryDequeue(out netEvent);
                int netcallTime = netEvent.GetNetcallTime();
                //Debug.Log(netcallTime + " "+ _clientTime);
                while (netcallTime > _clientTime)
                {
                    //yield return new WaitForSeconds(fixedTimePeriod * (netcallTime - _clientTime));
                    OnNetworkUpdate?.Invoke();
                    _clientTime++;
                    yield return new WaitForFixedUpdate();
                }
                //Debug.Log(netcallTime + " "+ _clientTime);
                UnityMainThreadDispatcher.Instance().Enqueue(netEvent.GetNetCallAction());
                //Debug.Log(_sessionTime + " " + _clientTime);
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
            yield return ProcessEventQueue(_netCallPreSyncQueue);
            //yield return new WaitForSeconds(1double / server tick rate); // this value meaning 1 / tickrate should come from the srver itself
            //done. time is set to entrance time which is lower than current time.
            //now that we're cought up with the previous events let's start catching up to current events
            yield return ProcessEventQueue(_liveEventQueue);
            //while
            //done. enghadr bayad sari poshte timere felie server bodoe le belakhare time ha yeki beshe va liste net call jadida khali besehe

            while (true)
            {
                if (_clientTime >= _sessionTime)
                    break;
                _clientTime++;
                OnNetworkUpdate?.Invoke();
                yield return new WaitForFixedUpdate();
                //Debug.Log("#" + _clientTime);
            }
            Time.timeScale = prevTime;
            //_clientTime = _sessionTime;
            Debug.Log("Game sync done" + _sessionTime + " " + _clientTime);
            IsLoadingGameSession = false;
            _missingNetEventsDownloaded = false;
            OnSessionSyncFinish?.Invoke();
        }
    }
}
