using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace ProgramChat_HomeworkWeek4
{
    public class ChatManager : MonoBehaviour
    {
        private WebSocket websocket;

        //Log in Panel
        public GameObject loginPanel;
        public Text inputIPAddressLoginPanel;
        public Text inputPortLoginPanel;

        private string iPAddress;
        private string port;
        private string username;

        //Always Stay
        public GameObject AlwaysGroup;
        public InputField inputUsername; //เห็นเมื่อล็อกอินแล้ว

        //Lobby Panel
        public GameObject lobbyPanel;
        public Text allRoomList;

        public Text titleRoomname;
        public InputField inputRoomName; //Request JoinRoom or CreateRoom

        public GameObject systemAlert;
        public Text systemAlertText;

        //Chat Panel
        public GameObject chatRoomPanel;
        public InputField inputChat;
        public GameObject chatBoxObject;
        public GameObject chatBoxContent;
        public Text roomNameHeadChat;

        //Client State
        private string clientState;
            //Idle ผู้ใช้ยังไม่ล็อกอิน
            //Lobby ผู้ใช้อยู่ในล็อบบี้
            //ChatRoom ผู้ใช้กำลังอยู่ในห้องแชท
        private string currentRoom; //ห้องในปัจจุบัน
        private string currentRequest;
            //RoomJoining
            //RoomCreating
        //private List<string> roomMember;


        //private string cutHerePls = "|| cutherepls ||";
        private List<Message> messagesList = new List<Message>();
        private List<MessageChatBox> MsgBoxList = new List<MessageChatBox>();

        private string tempData;

        void Start()
        {
            clientState = "Idle";
            currentRoom = string.Empty;
            currentRequest = string.Empty;
            iPAddress = "127.0.0.1";
            port = "5500";
            username = "Annonymous";
            tempData = string.Empty;
            loginPanel.SetActive(true);
            lobbyPanel.SetActive(false);
            systemAlert.SetActive(false);
            chatRoomPanel.SetActive(false);
            AlwaysGroup.SetActive(false);
        }

        private void Update()
        {
            if (tempData != string.Empty)
            {
                //print(tempData);

                MessageChat newMsg = MessageChat.JsontoMsgObj(tempData);

                //เข้าห้องสำเร็จ
                if (newMsg.eventName == EventTypes.JoinSuccess)
                {
                    systemAlertText.text = "Join room succcess";
                    systemAlert.SetActive(true);
                    JoinRoom(newMsg);
                    currentRequest = string.Empty;
                }

                //เข้าห้องไม่สำเร็จ ไม่มีห้องชื่อนี้
                else if (newMsg.eventName == EventTypes.JoinFailed)
                {
                    systemAlertText.text = "No room with\nthis name";
                    systemAlert.SetActive(true);
                    inputRoomName.text = string.Empty;
                    currentRequest = string.Empty;
                }

                //สร้างห้องสำเร็จ
                else if (newMsg.eventName == EventTypes.CreateSuccess)
                {
                    systemAlertText.text = "Create room succcess";
                    systemAlert.SetActive(true);
                    JoinRoom(newMsg);
                    currentRequest = string.Empty;
                }

                //สร้างห้องไม่สำเร็จ
                else if (newMsg.eventName == EventTypes.CreateFailed)
                {
                    systemAlertText.text = "Already have\nthis name";
                    systemAlert.SetActive(true);
                    currentRequest = string.Empty;
                }

                //รายชื่อห้อง
                else if (newMsg.eventName == EventTypes.AllRoomList)
                {
                    //print("AllRoom Receive");
                    if (newMsg.data == string.Empty)
                    {
                        allRoomList.text = "No Room"; 
                    }
                    else
                    {
                        allRoomList.text = newMsg.data;
                    }
                }

                //////ออกห้องสำเร็จ
                //////if(newMsg.Event == EventTypes.LeaveRoom)
                //////{

                //////}

                //รับข้อความแชท
                if (newMsg.eventName == EventTypes.ChatSystem)
                {
                    MessageBoxBuild(newMsg);
                }
                tempData = string.Empty;
            }
        }

        public void JoinRoomRequest()
        {
            if (inputRoomName.text == string.Empty)
            {
                systemAlertText.text = "Please input room name.";
                systemAlert.SetActive(true);
                return;
            }
            if (websocket.ReadyState == WebSocketState.Open)
            {
                clientState = StateTypes.RoomJoining;
                //print("Room Joining Request : " + inputRoomName.text);
                MessageChat newMsg = new MessageChat();
                newMsg.eventName = EventTypes.JoinRoom;
                newMsg.Room = inputRoomName.text;
                //newMsg.Sender = EventTypes.ChatSystem;
                //newMsg.data = "<color=red>" + username + "</color>" + " coming here.";
                websocket.Send(newMsg.MsgObjtoJson());
            }
        }
        public void CreateRoomRequest()
        {
            if (inputRoomName.text == string.Empty)
            {
                systemAlertText.text = "Please input room name.";
                systemAlert.SetActive(true);
                return;
            }

            if (websocket.ReadyState == WebSocketState.Open)
            {
                clientState = StateTypes.RoomCreating;
                //print("Room Creating Request : " + inputRoomName.text);
                MessageChat newMsg = new MessageChat();
                newMsg.eventName = EventTypes.CreateRoom;
                newMsg.Room = inputRoomName.text;
                newMsg.Sender = EventTypes.ChatSystem;
                newMsg.data = "<color=red>" + username + "</color>" + " coming here.";
                websocket.Send(newMsg.MsgObjtoJson());
            }
        }
        public void LeaveRoom()
        {
            if(clientState == StateTypes.ChatRoom)
            {
                MessageChat newMsg = new MessageChat();
                newMsg.eventName = EventTypes.LeaveRoom;
                newMsg.Room = currentRoom;
                newMsg.Sender = EventTypes.ChatSystem;
                newMsg.data = "<color=red>" + username + "</color>" + " leave the room.";

                websocket.Send(newMsg.MsgObjtoJson());
                foreach(MessageChatBox box in MsgBoxList)
                {
                    Destroy(box.textObject);
                }
                MsgBoxList.Clear();
                currentRoom = StateTypes.Lobby;

                loginPanel.SetActive(false);
                lobbyPanel.SetActive(true);
                chatRoomPanel.SetActive(false);
            }
        }
        private void JoinRoom(MessageChat newMsg)
        {
            print("Join Room");
            if (websocket.ReadyState == WebSocketState.Open)
            {
                clientState = StateTypes.ChatRoom;
                currentRoom = newMsg.Room;
                roomNameHeadChat.text = currentRoom;

                loginPanel.SetActive(false);
                lobbyPanel.SetActive(false);
                chatRoomPanel.SetActive(true);

                newMsg.eventName = EventTypes.ChatSystem;
                //newMsg.Room = inputRoomName.text;   
                newMsg.Sender = EventTypes.ChatSystem;
                newMsg.data = "<color=red>" + username + "</color>" + " coming here.";

                websocket.Send(newMsg.MsgObjtoJson());
            }
        }
        //private void LeaveRoomSuccess()
        //{

        //}

        public void AllRoomListRequest()
        {
            if (websocket.ReadyState == WebSocketState.Open)
            {
                MessageChat newMsg = new MessageChat();
                newMsg.eventName = EventTypes.AllRoomList;
                websocket.Send(newMsg.MsgObjtoJson());
            }
        }

        public void WebSocketConnection()
        {
            if (inputIPAddressLoginPanel.text != string.Empty)
            {
                iPAddress = inputIPAddressLoginPanel.text;
                inputIPAddressLoginPanel.text = string.Empty;
            }
            if (inputPortLoginPanel.text != string.Empty)
            {
                port = inputPortLoginPanel.text;
                inputPortLoginPanel.text = string.Empty;
            }

            //MessageBoxBuild("SystemMessage" + cutHerePls + "<color=orange>Joining</color>");
            //MessageBoxBuild("SystemMessage" + cutHerePls + "<color=orange>...</color>");

            Debug.Log($"Connect >>> ws://{iPAddress}:{port}/");

            websocket = new WebSocket($"ws://{iPAddress}:{port}/");
            websocket.OnMessage += OnMessage;
            websocket.Connect();
            clientState = StateTypes.Lobby;
            //if (websocket.ReadyState == WebSocketState.Open)
            //{
            //    MessageChat newMsg = new MessageChat();
            //    newMsg.StateType = StateTypes.Idle;
            //    newMsg.Data = "JoinChat";
            //    websocket.Send("SystemMessage" + cutHerePls + "<color=red>" + username + "</color>" + " coming here.");

            //}

            currentRoom = StateTypes.Lobby;
            AllRoomListRequest();

            loginPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            chatRoomPanel.SetActive(false);
            AlwaysGroup.SetActive(true);
        }

        public void SendMSG()
        {
            if ((websocket.ReadyState == WebSocketState.Open) && (inputChat.text != string.Empty))
            {
                MessageChat sendMsg = new MessageChat();
                sendMsg.eventName = EventTypes.ChatSystem;
                sendMsg.Room = currentRoom;
                sendMsg.Sender = username;
                sendMsg.data = inputChat.text;
                websocket.Send(sendMsg.MsgObjtoJson());
                inputChat.text = string.Empty;
            }
        }

        private void OnDestroy()
        {
            if ((websocket != null) && (websocket.ReadyState == WebSocketState.Open))
            {
                if (clientState == StateTypes.ChatRoom)
                {
                    MessageChat newMsg = new MessageChat();
                    newMsg.eventName = EventTypes.LeaveRoom;
                    newMsg.Room = currentRoom;
                    newMsg.Sender = EventTypes.ChatSystem;
                    newMsg.data = "<color=red>" + username + "</color>" + " leave the room.";

                    websocket.Send(newMsg.MsgObjtoJson());
                }
                websocket.Close();
            }
        }

        public void DisConnection()
        {
            if ((websocket != null) && (websocket.ReadyState == WebSocketState.Open))
            {
                LeaveRoom(); //ไม่ต้องกลัวจะออกทั้งๆที่ไม่ได้อยู่ในห้องแชท เพราะมีการเช็คในฟังก์ชั่นอยู่แล้ว

                websocket.Close();

                print("Disconnect");

                clientState = StateTypes.Login;
                currentRoom = string.Empty;
                currentRequest = string.Empty;

                //return to login panel
                loginPanel.SetActive(true);
                lobbyPanel.SetActive(false);
                chatRoomPanel.SetActive(false);
                AlwaysGroup.SetActive(false);
            }
            //Application.Quit();
        }

        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            tempData = messageEventArgs.Data;
        }

        private void MessageBoxBuild(MessageChat MsgChat)
        {
            print("Building");
            print(MsgChat.MsgObjtoJson());
            MessageChatBox newMsgBox = new MessageChatBox();
            newMsgBox.msgData = MsgChat;
            GameObject newBox = Instantiate(chatBoxObject, chatBoxContent.transform);
            newMsgBox.textObject = newBox.GetComponent<Text>();
            
            if(MsgChat.Sender == EventTypes.ChatSystem)
            {
                newMsgBox.textObject.text = MsgChat.data;
                newMsgBox.textObject.alignment = TextAnchor.MiddleCenter;
            }
            else
            {
                if(MsgChat.Sender == username)
                {
                    newMsgBox.textObject.text = "<color=green>" + MsgChat.Sender + "</color>: " + MsgChat.data;
                    newMsgBox.textObject.alignment = TextAnchor.MiddleRight;
                }
                else
                {
                    newMsgBox.textObject.text = "<color=darkblue>" + MsgChat.Sender + "</color>: " + MsgChat.data;
                    newMsgBox.textObject.alignment = TextAnchor.MiddleLeft;
                }
            }
            //var boxHeight = newBox.GetComponent<RectTransform>().rect.height;
            foreach (MessageChatBox box in MsgBoxList)
            {
                Transform rt = box.textObject.GetComponent<Transform>();
                rt.position = rt.position + new Vector3(0, 3f, 0);
            }
            MsgBoxList.Add(newMsgBox);

        }

        public void ChangeUsername()
        {
            if (inputUsername.text != string.Empty)
            {
                if ((clientState == StateTypes.ChatRoom) && (websocket.ReadyState == WebSocketState.Open))
                {
                    MessageChat newMsg = new MessageChat();
                    newMsg.eventName = EventTypes.ChatSystem;
                    newMsg.Room = currentRoom;
                    newMsg.Sender = EventTypes.ChatSystem;
                    newMsg.data = "<color=red>" + username + "</color> changed username >> " + "<color=red>" + inputUsername.text + "</color>";
                    websocket.Send(newMsg.MsgObjtoJson());
                }
                username = inputUsername.text;
                inputUsername.placeholder.GetComponent<Text>().text = username;
                //userNameTitle.text = "Username: " + username;
                inputUsername.text = string.Empty;
            }
            else
            {
                systemAlert.SetActive(true);
                systemAlertText.text = "Please input new username";
            }
        }
    }

    public class Message
    {
        public string Text;
        public Text textObject;
    }

    public class MessageChatBox
    {
        public MessageChat msgData;
        public Text textObject;
    }

    //public class RoomBox
    //{
    //    public Text TextObject;
    //    public int MemberAmount;
    //    public string RoomName;
    //    public RoomBox(string roomName, int memberAmount)
    //    {
    //        RoomName = roomName;
    //        MemberAmount = memberAmount;
    //    }
    //}

    public static class StateTypes
    {
        public const string Login = "Idle";
        public const string Lobby = "Lobby";
        public const string ChatRoom = "ChatRoom";
        public const string RoomJoining = "RoomJoining";
        public const string RoomCreating = "RoomCreating";
    }

    public static class EventTypes
    {
        public const string AllRoomList = "AllRoomList";
        public const string Connect = "Connect";
        public const string ChatMsg = "ChatMsg";
        public const string ChatSystem = "ChatSystem";
        
        public const string JoinRoom = "JoinRoom";
        public const string CreateRoom = "CreateRoom";
        public const string LeaveRoom = "LeaveRoom";
        public const string JoinSuccess = "JoinSuccess";
        public const string JoinFailed = "JoinFailed";
        public const string CreateSuccess = "CreateSuccess";
        public const string CreateFailed = "CreateFailed";
        public const string Disconnect = "Disconnect";
    }

}
