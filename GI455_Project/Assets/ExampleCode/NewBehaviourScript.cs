using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

namespace Test
{
    public class NewBehaviourScript : MonoBehaviour
    {
        private WebSocket websocket;
        private string tempData;
        public void Start()
        {
            tempData = "";
            WebSocketConnection();
            StudentData newS = new StudentData();
            print(JsonUtility.ToJson(newS));
        }
        private void Update()
        {
            if (tempData != string.Empty)
            {
                print(tempData);
                tempData = string.Empty;
            }
        }

        public void WebSocketConnection()
        {
            
            websocket = new WebSocket("ws://gi455-305013.an.r.appspot.com");
            websocket.OnMessage += OnMessage;
            websocket.Connect();

            StudentData newS = new StudentData();

            print("Send >>> " + JsonUtility.ToJson(newS));

            websocket.Send(JsonUtility.ToJson(newS));

        }

        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            tempData = messageEventArgs.Data;
        }

        private void OnDestroy()
        {
            if ((websocket != null) && (websocket.ReadyState == WebSocketState.Open))
            {
                websocket.Close();
            }
        }
    }

    public class StudentData
    {
        public string eventName = "GetStudentData";
        public string studentID = "1620706208";
    }

}