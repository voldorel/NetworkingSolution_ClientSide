using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Reflection;
using Newtonsoft.Json.Linq;


namespace MyNetwork
{
    public class GameSession : MonoBehaviour
    {
        protected delegate void ReceiveSessionText(string text);
        protected delegate void PlayerEntered(string text);
        protected delegate void PlayerLeft(string text);
        protected delegate void MatchStart(string text);
        protected delegate void MatchEnd();


        protected event ReceiveSessionText OnReceiveSessionText;
        protected event PlayerEntered OnPlayerEntered;
        protected event PlayerLeft OnPlayerLeft;
        protected event MatchStart OnMatchStart;
        protected event MatchEnd OnMatchEnd;

        public double TickrateFixedTime { 
            get
            {
                return Connection.Instance.GetTickrateFixedTime();
            }
            private set { }
        }



        public int SessionTime
        {
            get { return Connection.Instance.GetSessionTime(); }
        }


        protected void InitGameSession()
        {
            this.hideFlags = HideFlags.HideInInspector;

            Connection.Instance.OnReceiveSessionText += (e) => ReceiveSessionTextMethod(e);
            Connection.Instance.OnPlayerEnterSession += (e) => PlayerEnteredMethod(e);
            Connection.Instance.OnPlayerExitSession += (e) => PlayerLeftMethod(e);
            Connection.Instance.OnMatchStart += (e) => MatchStartMethod(e);
            Connection.Instance.OnNetworkFunctionCall += (e) => NetworkFunctionCallMethod(e);
            Connection.Instance.OnMatchEnd += MatchEndMethod;
            Connection.Instance.OnNetworkUpdate += () => {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    NetworkUpdate();
                });
            };
            Connection.Instance.OnSessionOutOfSync += () => {
                MessageView.ShowLoadingView(true);
            };
            Connection.Instance.OnSessionSyncFinish += () => {
                MessageView.ShowLoadingView(false);
            };


        }

        private void ReceiveSessionTextMethod(string sessionMessage)
        {
            OnReceiveSessionText?.Invoke(sessionMessage);
        }

        private void PlayerEnteredMethod(string userId)
        {
            OnPlayerEntered?.Invoke(userId);
        }

        private void PlayerLeftMethod(string userId)
        {
            OnPlayerLeft?.Invoke(userId);
        }

        private void MatchStartMethod(string args)
        {
            OnMatchStart?.Invoke(args);
        }

        public void InvokeMethod(string methodName, string args)
        {
            TurnBasedFighter turnBasedFighter = new TurnBasedFighter();
            turnBasedFighter.GetType().GetMethod(methodName)?.Invoke(this, new[] { args });
        }

        private void NetworkFunctionCallMethod(string args)
        {
            try
            {
                JObject keyValuePairs = JObject.Parse(args);
                string methodName = (string)keyValuePairs["methodName"];
                int argsCount = (int)keyValuePairs["methodArgsCount"];
                var parameterTypes = new Type[argsCount];
                object[] newParams = new object[argsCount];
                for (int i = 0; i < argsCount; i++)
                {
                    string paramValue = (string)keyValuePairs["methodArgs" + i];
                    Type type = Type.GetType((string)keyValuePairs["methodArgType" + i]);
                    parameterTypes[i] = type;
                    object convertedValue = Convert.ChangeType(paramValue, type);
                    newParams[i] = convertedValue;
                }

                List<MethodInfo> allMethodInfo = this.GetType().GetMethods(BindingFlags.NonPublic| BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList<MethodInfo>();
                //MethodInfo methodInfo = this.GetType().GetMethod(methodName, parameterTypes);
                var result = allMethodInfo.Find(i => i.Name.Equals(methodName)).Invoke(this, newParams);
            }
            catch
            {
                Debug.LogError("Parsing netcall failed! ... might be because of multiple methods with same name but different arguments");
            }
        }


        protected virtual void NetworkUpdate()
        {

        }


        //broken code... too advanced for my knowledge. needs help or you know...
        //delete third argument. It's just a placeholder to not be confused with the next method
        private void NetworkFunctionCallMethod<T>(string args, int useless) where T : IConvertible
        {
            try
            {
                JObject keyValuePairs = JObject.Parse(args);
                string methodName = (string)keyValuePairs["methodName"];
                T methodArgs = (T)Convert.ChangeType((string)keyValuePairs["methodArgs"], typeof(T));
                MethodInfo method = typeof(string).GetMethod(nameof(T));
                MethodInfo generic = method.MakeGenericMethod(typeof(T));
                generic.Invoke(this, null);
            } catch {
                Debug.LogError("Parsing netcall failed!");
            }
        }

        private void MatchEndMethod()
        {
            OnMatchEnd?.Invoke();
        }

        //broken code... too advanced for my knowledge. needs help or you know...
        //delete third argument. It's just a placeholder to not be confused with the next method
        protected async void NetCall<T>(string methodName, T methodArgs, int useless) where T : IConvertible
        {
            JObject keyValuePairs = new JObject();
            keyValuePairs["methodName"] = methodName;
            keyValuePairs["methodArgs"] = methodArgs + "";
            keyValuePairs["methodArgsType"] = typeof(T).ToString();
            Debug.Log(keyValuePairs["methodArgsType"]);

            await Connection.Instance.SendText(keyValuePairs.ToString(), "NetworkFunctionCall");
        }

        protected async void NetCall(string methodName, string methodArgs) 
        {
            JObject keyValuePairs = new JObject();
            keyValuePairs["methodName"] = methodName;
            keyValuePairs["methodArgs"] = methodArgs + "";
            await Connection.Instance.SendText(keyValuePairs.ToString(), "NetworkFunctionCall");
        }


        protected async void NetCall(string methodName, params IConvertible[] args)
        {
            JObject keyValuePairs = new JObject();
            keyValuePairs.Add("methodName", methodName);
            keyValuePairs.Add("methodArgsCount", args.Length);
            for (int i = 0; i < args.Length; i++)
            {
                keyValuePairs.Add("methodArgs" + i , args[i].ToString());
                keyValuePairs.Add("methodArgType" + i , args[i].GetType().ToString());
            }
            await Connection.Instance.SendText(keyValuePairs.ToString(), "NetworkFunctionCall");
        }


        protected void SynchronizeGame()
        {
            //StartCoroutine(SynchronizeCoroutine());
        }

        
    }
}
