using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

namespace MultiPlayerExampleWeek8
{
    public class SocketConnect : MonoBehaviour
    {
        private WebSocket ws;
        public delegate void DelegateHandler(string msg);

        public event DelegateHandler OnConnectionSuccess;
        public event DelegateHandler OnConnectionFail;
        public event DelegateHandler OnReceiveMessage;
        public event DelegateHandler OnCreateRoom;
        public event DelegateHandler OnJoinRoom;
        public event DelegateHandler OnLeaveRoom;
        public event DelegateHandler OnLogin;
        public event DelegateHandler OnRegister;
        public event DelegateHandler OnDisconnect;

        private bool isConnection;

        public Room currentRoom;

        private List<string> messageQueue = new List<string>();

        private List<NetworkDataOption.ReplicateObjact> replicateSend = new List<NetworkDataOption.ReplicateObjact>();

        public static SocketConnect instance;

        public void Awake()
        {
            instance = this;
        }
        public void Connect(string ip, int port)
        {
            string url = $"ws://{ip}:{port}/";
            InternalConnect(url);
        }

        public void Connect()
        {
            //string url = "ws://gi455-305013.an.r.appspot.com/";
            string url = "ws://127.0.0.1:8080/";
            InternalConnect(url);
        }

        private void InternalConnect(string url)
        {
            if (isConnection)
                return;

            isConnection = true;

            ws = new WebSocket(url);
            //ws.OnMessage += OnMessage;
            ws.Connect();
            StartCoroutine(WaitingConnectionState());
        }


        private IEnumerator WaitingConnectionState()
        {
            yield return new WaitForSeconds(1.0f);
            if (ws.ReadyState == WebSocketState.Open)
            {
                if (OnConnectionSuccess != null)
                    OnConnectionSuccess("Success");
            }
            else
            {
                if (OnConnectionFail != null)
                    OnConnectionFail("Fail");
            }
            isConnection = false;
        }

        private IEnumerator IEUpdateReplicateObject()
        {
            float duration = 1.0f; //ส่งอัพเดททุกๆ 1 วินาที
            WaitForSeconds waitForSec = new WaitForSeconds(duration);
            while (true)
            {
                string toJson = JsonUtility.ToJson(replicateSend);

                SendReplicateData(toJson);

                yield return waitForSec;
            }
        }

        public void Disconnect()
        {
            if (ws != null)
                ws.Close();
        }

        public bool IsConnected()
        {
            if (ws == null)
                return false;
            return ws.ReadyState == WebSocketState.Open;
        }

        public void CreateRoom(Room.RoomOption roomOption)
        {
            NetworkDataOption.EventSendCreateRoom eventData = new NetworkDataOption.EventSendCreateRoom();

            eventData.eventName = "CreateRoom";
            eventData.roomOption = roomOption;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void JoinRoom(string roomName)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();

            eventData.eventName = "JoinRoom";
            eventData.data = roomName;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void LeaveRoom()
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();

            eventData.eventName = "LeaveRoom";
            eventData.data = "";

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void Login(string userId, string password)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();

            eventData.eventName = "Login";
            eventData.data = userId + "#" + password;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void Register(string userId, string password)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();

            eventData.eventName = "Login";
            eventData.data = userId + "#" + password + "#" + name;

            string toJson = JsonUtility.ToJson(eventData);

            ws.Send(toJson);
        }

        public void SendReplicateData(string jsonStr)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();
            eventData.eventName = "ReplicateData";
            eventData.data = jsonStr;

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void SendMessage(string data)
        {
            if (!IsConnected())
                return;

            NetworkDataOption.EventCallbackGeneral msgEventData = new NetworkDataOption.EventCallbackGeneral();
            msgEventData.eventName = "SendMessage";
            msgEventData.data = data;

            string toJson = JsonUtility.ToJson(msgEventData);

            ws.Send(toJson);
        }

        public void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (ws != null)
                ws.Close();
        }



        // Update is called once per frame
        void Update()
        {
            if (messageQueue.Count > 0)
            {

            }
        }

        private void NotifyCallback(string callbackData)
        {
            Debug.Log("OnMesasage : " + callbackData);
            EventServer recieveEvent = JsonUtility.FromJson<EventServer>(callbackData);
            Debug.Log(recieveEvent.eventName);
            switch (recieveEvent.eventName)
            {
                case "CreateRoom":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        Internal_CreateRoom(receiveEventGeneral.data);
                        if (OnCreateRoom != null)
                            OnCreateRoom(receiveEventGeneral.data);
                        break;
                    }
                case "JoinRoom":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        if (OnJoinRoom != null)
                            OnJoinRoom(receiveEventGeneral.data);
                        break;
                    }
                case "LeaveRoom":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        if (OnLeaveRoom != null)
                            OnLeaveRoom(receiveEventGeneral.data);
                        break;
                    }
                case "SendMessage":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        if (OnReceiveMessage != null)
                            OnReceiveMessage(receiveEventGeneral.data);
                        break;
                    }
                case "Login":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        if (OnLogin != null)
                            OnLogin(receiveEventGeneral.data);
                        break;
                    }
                case "Register":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        if (OnRegister != null)
                            OnRegister(receiveEventGeneral.data);
                        break;
                    }
            }
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            messageQueue.Add(messageEventArgs.Data);
        }

        private void Internal_CreateRoom(string data)
        {
            Room.RoomOption roomOption = JsonUtility.FromJson<Room.RoomOption>(data);
            if (roomOption != null && currentRoom == null) //ยังไม่เคยมีห้องมาก่อน
            {
                currentRoom = new Room();
                currentRoom.roomOption = roomOption;

                StartCoroutine(IEUpdateReplicateObject());
            }
        }
    }
}