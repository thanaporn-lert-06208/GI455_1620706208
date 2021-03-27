using System;
using UnityEngine;
using System.Collections.Generic;

namespace MultiPlayerExampleWeek9
{
    public class NetworkDataOption
    {
        [Serializable]
        public class ReplicateObject //หมายถึง obj ที่มีการทำซ้ำบ่อยๆ
        {
            public string objectID;
            public string ownerID;
            public string prefName;
            public Vector3 position;
            public Quaternion rotation;
            public NetworkObject netObj;
        }

        [SerializeField]
        public class EventCallbackGeneral
        {
            public string eventName;
            public string data;
        }

        [SerializeField]
        public class ReplicateObjectList
        {
            //สร้างคลาสสำหรับเก็บ ReplicateObj ไว้ส่งเป็น JsonStr
            public List<ReplicateObject> replicateObjectList = new List<ReplicateObject>();
        }

        public class EventSendReplicate : EventCallbackGeneral
        {
            public string roomName;
        }

        [Serializable] //ใส่ไว้ให้แปลงเป็น Json ได้
        public class EventSendCreateRoom : EventCallbackGeneral
        {
            public Room.RoomOption roomOption;
        }
    }

    public class Room
    {
        [Serializable]
        public class RoomOption
        {
            public string roomName;

        }

        public RoomOption roomOption;
    }

    public class EventServer
    {
        public string eventName;
    }

    
}