using System.Collections;
using System.Collections.Generic;
using DreamNet.Utils;
using UnityEditor;
using UnityEngine;

namespace DreamNet.Config
{
    //[CreateAssetMenu(fileName = "DreamNetConfig", menuName = "DreamByte/DreamConfigScriptableObject", order = 1)]
    public class DreamNetConfig : ScriptableObject
    {
        public string ServerAddress;
        [TextArea(2, 2)] public string LoginToken;

        public void DoClearLoginToken()
        {
            DreamUtils.DeleteDreamToken();
        }

        public void DoRefreshLoginTokenText()
        {
            LoginToken = DreamUtils.GetDreamToken();
        }
    }
}
