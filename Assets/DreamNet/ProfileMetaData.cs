using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace DreamNet
{
    public class ProfileMetaData
    {
        public string NickName;
        public UserInfo UserInfo;

        public void Init(string nickName, string profileInfo)
        {
            this.NickName = nickName;
            UserInfo = new UserInfo(JObject.Parse(profileInfo));
        }
        public void ChangeNickName(string nickName)
        {
            JObject data = new JObject();
            data.Add("NickName",nickName);
            Connection.Instance.SendText(JsonConvert.SerializeObject(data), "ChangeUserNickName", () =>
            {
                Debug.Log("Changed Name Successfully !!!");
                NickName = nickName;
            });
        }
        public void ChangeAvatarId(int avatarId)
        {
            JObject data = new JObject();
            data.Add("AvatarId",avatarId);
            Connection.Instance.SendText(JsonConvert.SerializeObject(data), "ChangeAvatarId", () =>
            {
                Debug.Log("Changed Avatar Id Successfully !!!");
            });
        }

        public void OnChangeUserInfo(string userInfo)
        {
            UserInfo = new UserInfo(JObject.Parse(userInfo));
        }

        public void OnChangeUserNickName(string nickName)
        {
            NickName = nickName;
        }
    }
    
    public class UserInfo
    {
        private JObject _data;

        public UserInfo(JObject jObject)
        {
            this._data = jObject;
        }
        public int AvatarId
        {
            get
            {
                if (_data.ContainsKey("AvatarId"))
                {
                    return int.Parse(_data["AvatarId"].ToString());
                }
                else return 0;
            }
        }
    }
}