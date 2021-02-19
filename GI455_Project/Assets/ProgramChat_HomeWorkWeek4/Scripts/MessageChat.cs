using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProgramChat_HomeworkWeek4
{
    public class MessageChat
    {
        public string eventName;
        //Lobby ผู้ใช้ในล็อบบี้
        //ChatRoom ผู้ใช้ในห้องแชท
        public string Room;
        public string Sender;
        public string data;

        //public MessageChat()
        //{
        //    //StateType = string.Empty;
        //    //Room = string.Empty;
        //    //Sender = string.Empty;
        //    //Data = string.Empty;
        //}

        public static MessageChat JsontoMsgObj(string jsonString)
        {
            return JsonUtility.FromJson<MessageChat>(jsonString);
        }

        public static string MsgObjtoJson(MessageChat msgObj)
        {
            return JsonUtility.ToJson(msgObj);
        }
        public string MsgObjtoJson()
        {
            return JsonUtility.ToJson(this);
        }
    }

}