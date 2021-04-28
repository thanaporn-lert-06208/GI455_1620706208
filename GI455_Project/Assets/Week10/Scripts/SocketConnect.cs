using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System.Reflection;


namespace MultiPlayerExampleWeek10
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

        private List<byte[]> binaryQueue = new List<byte[]>();

        private NetworkDataOption.ReplicateObjectList replicateListSend = new NetworkDataOption.ReplicateObjectList();

        private NetworkDataOption.ReplicateObjectList replicateListRecv = new NetworkDataOption.ReplicateObjectList();

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
            while (true)
            {
                for (int i = 0; i < replicateListSend.replicateObjectList.Count; ++i)
                {
                    if(replicateListSend.replicateObjectList[i].isMarkRemove == false && 
                        replicateListSend.replicateObjectList[i].netObj != null)
                    {
                        replicateListSend.replicateObjectList[i].position = replicateListSend.replicateObjectList[i].netObj.transform.position;
                        replicateListSend.replicateObjectList[i].rotation = replicateListSend.replicateObjectList[i].netObj.transform.rotation;
                    }
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

        //ok
        public void SendReplicateData(string jsonStr)
        {
            NetworkDataOption.EventSendReplicate eventData = new NetworkDataOption.EventSendReplicate();
            eventData.eventName = "ReplicateData";
            eventData.data = jsonStr;
            eventData.roomName = currentRoom.roomOption.roomName;
            
            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);

            //ส่งข้อมูลเป็น byte array
            //ws.Send(replicateListSend.ToByteArr());
        }

        //public void SendFunction(NetworkObject netObj, string methodName, object objTarget ,Types fromType, params object[] parameters)
        //{
        //    {
        //    //    //ไว้เรียก string กับ object ที่ส่งเข้ามาได้
        //    //    //เรียกฟังก์ชั่นผ่าน String อยากเรียกอะไรก็ใส่ชื่อ mothod ไป จะได้ mothod info ที่เป็น array มา
        //    //    MethodInfo method = fromType.GetMethod(methodName);
        //    //    method.Invoke(objTarget, parameters);
        //    }

        //    NetworkDataOption.EventSendFunction eventData = new NetworkDataOption.EventSendFunction();
        //    eventData.objectID = netObj.replicateData.objectID;
        //    eventData.methodName = methodName;
        //    eventData.typeName = fromType.Name;
        //    eventData.objects = parameters;
        //    var byteSend = NetworkDataOption.toByteArr
        //}

        public void SpawnNetworkObject(string prefName, Vector3 position, Quaternion rotation)
        {
            NetworkDataOption.EventCallbackGeneral eventData = new NetworkDataOption.EventCallbackGeneral();
            eventData.eventName = "RequestUIDObject";

            string toJson = JsonUtility.ToJson(eventData);
            ws.Send(toJson);

            if (tempSpawnNetworkObj == null)
            {
                tempSpawnNetworkObj = new NetworkDataOption.ReplicateObject();
            }

            tempSpawnNetworkObj.prefName = prefName;
            tempSpawnNetworkObj.position = position;
            tempSpawnNetworkObj.rotation = rotation;
        }

        public void DestroyNetworkObject(string objectID)
        {
            for(int i = 0; i > replicateListSend.replicateObjectList.Count; i++)
            {
                if(replicateListSend.replicateObjectList[i].objectID == objectID)
                {
                    if(replicateListSend.replicateObjectList[i].netObj != null)
                    {
                        Destroy(replicateListSend.replicateObjectList[i].netObj.gameObject);
                    }
                    replicateListSend.replicateObjectList[i].isMarkRemove = true;
                    break;
                }
            }
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

            //แปลง msgEventData ในรูปแบบของ byte array
            {
                //var binFormatter = new BinaryFormatter();
                //var mStream = new MemoryStream();
                //binFormatter.Serialize(mStream, msgEventData);
                //ws.Send(mStream.ToArray());
            }
        }


        public void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (ws != null)
                ws.Close();
        }

        void Update()
        {
            if (messageQueue.Count > 0)
            {
                NotifyCallback(messageQueue[0]);
                messageQueue.RemoveAt(0);
            }
            if (binaryQueue.Count > 0)
            {
                NotifyBinaryCallback(binaryQueue[0]);
                binaryQueue.RemoveAt(0);
            }
        }

        private void NotifyCallback(string callbackData)
        {
            //Debug.Log("OnMesasage : " + callbackData);
            EventServer receiveEvent = JsonUtility.FromJson<EventServer>(callbackData);
            Debug.Log(receiveEvent.eventName);
            switch (receiveEvent.eventName)
            {
                case "Connect":
                    {
                        NetworkDataOption.EventCallbackGeneral receiveEventGeneral = JsonUtility.FromJson<NetworkDataOption.EventCallbackGeneral>(callbackData);
                        clientID = receiveEventGeneral.data;
                        break;
                    }
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

        private void NotifyBinaryCallback(byte[] byteArr)
        {
            NetworkDataOption.ReplicateObjectList toReplicateList = new NetworkDataOption.ReplicateObjectList();
            toReplicateList = toReplicateList.FromByteArr(byteArr);

            toReplicateList.replicateObjectList[0].position = Vector3.zero;
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            messageQueue.Add(messageEventArgs.Data);

            //messageEventArgs.RawData = รับ data เป็น byte array
            //if (messageEventArgs.IsText)
            //{
            //    messageQueue.Add(messageEventArgs.Data);
            //}
            //if (messageEventArgs.IsBinary)
            //{
            //    binaryQueue.Add(messageEventArgs.RawData); //รับ data เป็น byte array
            //}
        }

        private void Internal_CreateRoom(string data)
        {
            Room.RoomOption roomOption = JsonUtility.FromJson<Room.RoomOption>(data);
            if (roomOption != null && currentRoom == null) //ยังไม่เคยเข้าห้องมาก่อน
            {
                currentRoom = new Room();
                currentRoom.roomOption = roomOption;

                StartCoroutine(IEUpdateReplicateObject());
            }
        }

        private void Internal_JoinRoom(string data)
        {
            Internal_CreateRoom(data);
        }

        private void Internal_ReplicateData(string data)
        {
            NetworkDataOption.ReplicateObjectList toReplicateList = JsonUtility.FromJson<NetworkDataOption.ReplicateObjectList>(data);

            //for Delete
            {
                /*
                bool isFoundRemoveObj = false;
                for (int i = 0; i < replicateListRecv.replicateObjectList.Count; i++)
                {
                    bool isRemoveObject = true;
                    NetworkDataOption.ReplicateObject replicateObjClient = replicateListRecv.replicateObjectList[i];

                    for (int j = 0; j < toReplicateList.replicateObjectList.Count; j++)
                    {
                        NetworkDataOption.ReplicateObject replicateObjServer = replicateListRecv.replicateObjectList[i];

                        if (replicateObjServer.objectID == replicateObjClient.objectID)
                        {
                            isRemoveObject = false;
                            break;
                        }
                    }
                    if (isRemoveObject == true)
                    {
                        Destroy(replicateObjClient.netObj.gameObject);
                        replicateListRecv.replicateObjectList.RemoveAt(i);
                        isFoundRemoveObj = true;
                        break;
                    }
                }
                if (isFoundRemoveObj == true)
                {
                    return;
                }
                */
            }

            //for Create
            for (int i = 0; i< toReplicateList.replicateObjectList.Count;i++)
            {
                NetworkDataOption.ReplicateObject replicateObj = toReplicateList.replicateObjectList[i];
                bool isNewObject = true;
                bool isNotSendObj = true;

                string objectID = replicateObj.objectID;

                for(int j = 0; j < replicateListRecv.replicateObjectList.Count;j++)
                {
                    if(replicateListRecv.replicateObjectList[j].objectID == objectID)
                    {
                        if(replicateObj.isMarkRemove == false)
                        {
                            isNewObject = false;
                            replicateListRecv.replicateObjectList[j].position = replicateObj.position;
                            replicateListRecv.replicateObjectList[j].rotation = replicateObj.rotation;

                            replicateListRecv.replicateObjectList[j].netObj.correctPosition = replicateObj.position;
                            replicateListRecv.replicateObjectList[j].netObj.correctRotation = replicateObj.rotation;
                        }
                        //else if(replicateObj.isMarkRemove == true && replicateObj.netObj != null)
                        else if(replicateObj.isMarkRemove == true && replicateListRecv.replicateObjectList[j].netObj != null)
                        {//1.07.20
                            //ชั่วโมงที่ 1.33.07
                            Destroy(replicateListRecv.replicateObjectList[j].netObj.gameObject);
                            replicateListRecv.replicateObjectList.RemoveAt(j);
                        }
                        isNewObject = false;
                    }
                }

                for(int j = 0; j < replicateListSend.replicateObjectList.Count; j++)
                {
                    if(replicateListSend.replicateObjectList[j].objectID == objectID)
                    {
                        isNotSendObj = false;
                        if(replicateListSend.replicateObjectList[j].isMarkRemove == true)
                        {
                            if (replicateListSend.replicateObjectList[j].netObj != null)
                            {
                                Destroy(replicateListSend.replicateObjectList[j].netObj.gameObject);
                            }
                            replicateListSend.replicateObjectList.RemoveAt(j);
                        }
                        break;
                    }
                }

                if(isNewObject == true && isNotSendObj == true && replicateObj.isMarkRemove == false)
                {
                    GameObject newGameObject = Instantiate(Resources.Load(replicateObj.prefName) as GameObject);
                    NetworkObject netObj = newGameObject.GetComponent<NetworkObject>();
                    netObj.ownerID = replicateObj.ownerID;
                    netObj.objectID = replicateObj.objectID;
                    newGameObject.transform.position = replicateObj.position;
                    newGameObject.transform.rotation = replicateObj.rotation;
                    replicateObj.netObj = netObj;
                    replicateListRecv.replicateObjectList.Add(replicateObj);

                }
            }

        }

        private void Internal_SpawnNetworkObject(string data)
        {
            if (tempSpawnNetworkObj == null || tempSpawnNetworkObj.prefName == "")
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
            NetworkObject netObj = newGameObject.GetComponent<NetworkObject>();
            netObj.ownerID = newReplicateObject.ownerID;
            netObj.objectID = newReplicateObject.objectID;
            newGameObject.transform.position = newReplicateObject.position;
            newGameObject.transform.rotation = newReplicateObject.rotation;
            newReplicateObject.netObj = netObj;

            replicateListSend.replicateObjectList.Add(newReplicateObject);

            tempSpawnNetworkObj.prefName = "";

        }


    }
}