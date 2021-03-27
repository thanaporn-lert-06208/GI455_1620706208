using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

namespace MultiPlayerExampleWeek9
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

        private NetworkDataOption.ReplicateObjectList replicateListSend = new NetworkDataOption.ReplicateObjectList();

        private NetworkDataOption.ReplicateObject tempSpawnNetworkObj;

        public string clientID;

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
            string url = "ws://127.0.0.1:5500/";
            InternalConnect(url);
        }

        private void InternalConnect(string url)
        {
            if (isConnection)
                return;

            isConnection = true;

            ws = new WebSocket(url);
            ws.OnMessage += OnMessage;
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
            while(true)
            {
                for(int i = 0; i < replicateListSend.replicateObjectList.Count; ++i)
                {
                    replicateListSend.replicateObjectList[i].position = replicateListSend.replicateObjectList[i].netObj.transform.position;
                    replicateListSend.replicateObjectList[i].rotation = replicateListSend.replicateObjectList[i].netObj.transform.rotation;
                }

                string toJson = JsonUtility.ToJson(replicateListSend);

                Debug.Log(toJson);

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

        public void Login (string userId, string password)
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
            NetworkDataOption.EventSendReplicate eventData = new NetworkDataOption.EventSendReplicate();
            eventData.eventName = "ReplicateData";
            eventData.data = jsonStr;
            eventData.roomName = currentRoom.roomOption.roomName;

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);
        }

        public void SpawnNetworkObject(string prefName, Vector3 position, Quaternion rotation)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();
            eventData.eventName = "RequestUIDObject";

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);

            if(tempSpawnNetworkObj == null)
            {
                tempSpawnNetworkObj = new NetworkDataOption.ReplicateObject();
            }

            tempSpawnNetworkObj.prefName = prefName;
            tempSpawnNetworkObj.position = position;
            tempSpawnNetworkObj.rotation = rotation;
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

        void Update()
        {
            if(messageQueue.Count > 0)
            {
                NotifyCallback(messageQueue[0]);
                messageQueue.RemoveAt(0);
            }
        }

        private void NotifyCallback(string callbackData)
        {
            Debug.Log("OnMesasage : " + callbackData);
            EventServer receiveEvent = JsonUtility.FromJson<EventServer>(callbackData);
            Debug.Log(receiveEvent.eventName);
            switch(receiveEvent.eventName)
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
                case "ReplicateData":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        Internal_ReplicateData(receiveEventGeneral.data);
                        break;
                    }
                case "RequestUIDObject":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        Internal_SpawnNetworkObject(receiveEventGeneral.data);
                        break;
                    }
                default:
                    break;
            }
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            messageQueue.Add(messageEventArgs.Data);
        }

        private void Internal_CreateRoom(string data)
        {
            Room.RoomOption roomOption = JsonUtility.FromJson<Room.RoomOption>(data);
            if(roomOption != null && currentRoom == null) //ยังไม่เคยมีห้องมาก่อน
            {
                currentRoom = new Room();
                currentRoom.roomOption = roomOption;

                StartCoroutine(IEUpdateReplicateObject());
            }
        }
        
        private void Internal_ReplicateData(string data)
        {
            //Debug.Log("Internal_Replicate : " + data);

        }

        private void Internal_SpawnNetworkObject(string data)
        {
            if(tempSpawnNetworkObj == null || tempSpawnNetworkObj.prefName == "")
            {
                return;
            }

            string uid = data;

            NetworkDataOption.ReplicateObject newReplicateObject = new NetworkDataOption.ReplicateObject();
            newReplicateObject.ownerID = clientID;
            newReplicateObject.objectID = uid;
            newReplicateObject.prefName = tempSpawnNetworkObj.prefName;
            newReplicateObject.position = tempSpawnNetworkObj.position;
            newReplicateObject.rotation = tempSpawnNetworkObj.rotation;

            GameObject newGameObject = Instantiate(Resources.Load(newReplicateObject.prefName) as GameObject);
            newGameObject.transform.position = newReplicateObject.position;
            newGameObject.transform.rotation = newReplicateObject.rotation;
            //newReplicateObject.netObj = newGameObject;

            replicateListSend.replicateObjectList.Add(newReplicateObject);

            tempSpawnNetworkObj.prefName = "";
            
        }


    }
}