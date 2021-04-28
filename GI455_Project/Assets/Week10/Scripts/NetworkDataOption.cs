using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MultiPlayerExampleWeek10
{
    public class NetworkDataOption
    {
        

        [Serializable]
        public class ReplicateObject //หมายถึง obj ที่มีการทำซ้ำบ่อยๆ
        {
            public string objectID;
            public string ownerID;
            public string prefName;

            public bool isMarkRemove;
            public Vector3 position;
            public Quaternion rotation;

            public NetworkObject netObj;

        }

        [SerializeField]
        public class ReplicateObjectList
        {
            //สร้างคลาสสำหรับเก็บ ReplicateObj ไว้ส่งเป็น JsonStr
            public List<ReplicateObject> replicateObjectList = new List<ReplicateObject>();

            public byte[] ToByteArr()
            {
                var binFormatter = new BinaryFormatter();
                var mStream = new MemoryStream();
                binFormatter.Serialize(mStream, this);
                return mStream.ToArray();
            }

            public ReplicateObjectList FromByteArr(byte[] byteArr)
            {
                var mStream = new MemoryStream();
                var binFormatter = new BinaryFormatter();

                mStream.Write(byteArr, 0, byteArr.Length);
                mStream.Position = 0;
                return binFormatter.Deserialize(mStream) as ReplicateObjectList;
            }
            //public static T FromByteArr<T>(byte[] byteArr)
            //{
            //    var mStream = new MemoryStream();
            //    var binFormatter = new BinaryFormatter();

            //    mStream.Write(byteArr, 0, byteArr.Length);
            //    mStream.Position = 0;
            //    return (T)binFormatter.Deserialize(mStream);
            //}
        }

        [SerializeField]
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

        public class EventSendReplicate : EventCallbackGeneral
        {
            public string roomName;
        }

        [Serializable]
        public class EventSendFunction
        {
            public string objectID;
            public string methodName;
            public string typeName;
            public object[] objects;
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