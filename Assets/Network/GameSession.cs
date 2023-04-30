using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

namespace MyNetwork
{
    public class GameSession : MonoBehaviour
    {
        public delegate void ReceiveSessionText(string text);
        public delegate void MemberEntered();
        public delegate void MemberLeft();
        public delegate void MatchStart(string text);
        public delegate void NetworkFunctionCallDelegate(string text);
        public delegate void MatchEnd();


        public event ReceiveSessionText OnReceiveSessionText;
        public event MemberEntered OnMemberEntered;
        public event MemberLeft OnMemberLeft;
        public event MatchStart OnMatchStart;
        public event NetworkFunctionCallDelegate OnNetworkFunctionCall;
        public event MatchEnd OnMatchEnd;


        public int SessionTime
        {
            get { return Connection.Instance.GetSessionTime(); }
        }


        public void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
            Connection.Instance.OnReceiveSessionText += (e) => ReceiveSessionTextMethod(e);
            Connection.Instance.OnMemberEntered += MemberEnteredMethod;
            Connection.Instance.OnMemberLeft += MemberLeftMethod;
            Connection.Instance.OnMatchStart += (e) => MatchStartMethod(e);
            Connection.Instance.OnNetworkFunctionCall += (e) => NetworkFunctionCallMethod(e);
            Connection.Instance.OnMatchEnd += MatchEndMethod;
        }

        private void ReceiveSessionTextMethod(string sessionMessage)
        {
            OnReceiveSessionText?.Invoke(sessionMessage);
        }

        private void MemberEnteredMethod()
        {
            OnMemberEntered?.Invoke();
        }

        private void MemberLeftMethod()
        {
            OnMemberLeft?.Invoke();
        }

        private void MatchStartMethod(string args)
        {
            OnMatchStart?.Invoke(args);
        }

        private void NetworkFunctionCallMethod(string args)
        {
            OnNetworkFunctionCall?.Invoke(args);
        }

        private void MatchEndMethod()
        {
            OnMatchEnd?.Invoke();
        }
    }
}
