using System;
using UnityEngine;
using System.Collections.Generic;
namespace MultiPlayerExampleWeek8
{
    public class NetworkDataOption
    {
        [Serializable]
        public class ReplicateObjact
        {
            public string objectID;
            public string ownerID;
            public string prefName;

            public Vector3 position; 
        }

        [SerializeField]
        public class ReplicateObjectList
        {
            public List<ReplicateObjact> replicateObjectList = new List<ReplicateObjact>();
        }

        public class EventCallbackGeneral
        {
            public string eventName;
            public string data;
        }

        [Serializable] //ใส่ไว้ให้แปลงเป็น Json ได้
        public class EventSendCreateRoom : EventCallbackGeneral
        {
            public Room.RoomOption roomOption;
        }
    }
    public class Room
    {
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