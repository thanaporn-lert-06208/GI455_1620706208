using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace ProgramChat_HomeworkWeek5
{
    public class ChatManager : MonoBehaviour
    {
        private WebSocket websocket;

        private string iPAddress;
        private string port;
        private string username;

        public GameObject connectButton;

        UserInfo userInfo;

        //Log in Panel
        public GameObject loginPanel;
        public InputField loginInputUserID;
        public InputField loginInputPassword;

        //Register Panel
        public GameObject registerPanel;
        public InputField registerInputUserID;
        public InputField registerInputUsername;
        public InputField registerInputPassword;
        public InputField registerInputRePassword;

        //Always Stay
        public GameObject AlwaysGroup;
        public InputField inputUsername; //เห็นเมื่อล็อกอินแล้ว

        //Lobby Panel
        public GameObject lobbyPanel;
        public Text allRoomList;

        public InputField inputRoomName; //Request JoinRoom or CreateRoom

        //SystemAlert Panel
        public GameObject systemAlert;
        public Text systemAlertText;

        //Loading Panel
        public GameObject loadingPanel;

        //Chat Panel
        public GameObject chatRoomPanel;
        public InputField inputChat;
        public GameObject chatBoxObject;
        public GameObject chatBoxContent;
        public Text roomNameHeadChat;

        //Client State
        private string clientState;
        private string currentRoom; //ห้องในปัจจุบัน

        private List<MessageChatBox> MsgBoxList = new List<MessageChatBox>();
        private string tempData;

        void Start()
        {
            clientState = StateTypes.Idle;
            currentRoom = string.Empty;
            iPAddress = "127.0.0.1";
            port = "5500";
            username = "Annonymous";
            tempData = string.Empty;
            userInfo = new UserInfo();
            loginPanel.SetActive(false);
            registerPanel.SetActive(false);
            lobbyPanel.SetActive(false);
            chatRoomPanel.SetActive(false);
            AlwaysGroup.SetActive(false);
            systemAlert.SetActive(false);
            loadingPanel.SetActive(false);
            connectButton.SetActive(true);
        }

        private void Update()
        {
            if (tempData != string.Empty)
            {
                print(tempData);

                // User in Lobby or ChatRoom
                if (currentRoom != string.Empty) 
                {
                    //print("Lobby | ChatRoom");
                    MessageChat newMsg = MessageChat.JsontoMsgObj(tempData);

                    switch(newMsg.eventName)
                    {
                        //รับข้อความแชท
                        case EventTypes.ChatSystem:
                            MessageBoxBuild(newMsg);
                            break;
                        case StateTypes.JoinRoom:
                            //เข้าห้องสำเร็จ
                            if (newMsg.data == EventTypes.Success)
                            {
                                systemAlertText.text = "Join room succcess";
                                systemAlert.SetActive(true);
                                JoinRoom(newMsg);
                                loadingPanel.SetActive(false);
                            }
                            //เข้าห้องไม่สำเร็จ ไม่มีห้องชื่อนี้ newMsg.data == EventTypes.Failed
                            else
                            {
                                systemAlertText.text = "Join Room Failed\nNo room with this name";
                                systemAlert.SetActive(true);
                                clientState = StateTypes.Lobby;
                                loadingPanel.SetActive(false);
                            }
                            break;
                        case StateTypes.CreateRoom:
                            //สร้างห้องสำเร็จ
                            if (newMsg.data == EventTypes.Success)
                            {
                                systemAlertText.text = "Create room succcess";
                                systemAlert.SetActive(true);
                                JoinRoom(newMsg);
                                loadingPanel.SetActive(false);
                                inputRoomName.text = string.Empty;
                            }

                            //สร้างห้องไม่สำเร็จ newMsg.data == EventTypes.Failed
                            else
                            {
                                systemAlertText.text = "Already have this name";
                                systemAlert.SetActive(true);
                                clientState = StateTypes.Lobby;
                                loadingPanel.SetActive(false);
                            }
                            break;
                        case EventTypes.AllRoomList:
                            //print("AllRoom Receive");
                            if (newMsg.data == string.Empty)
                            {
                                allRoomList.text = "No Room";
                            }
                            else
                            {
                                allRoomList.text = newMsg.data;
                            }
                            loadingPanel.SetActive(false);
                            break;
                        case EventTypes.ErrorAlert:
                            systemAlertText.text = newMsg.data;
                            break;
                        default:
                            print("Error " + currentRoom);
                            break;
                    }
                }
                
                // Login | Register
                else
                {
                    //print("Login | Register");
                    UserInfo returnUserInfo = JsonUtility.FromJson<UserInfo>(tempData);
                    // Login
                    if (returnUserInfo.eventName == StateTypes.Login)
                    {
                        if (returnUserInfo.data1 == EventTypes.Success)
                        {
                            print("Login Success");
                            //returnUserInfo.data2 == Username
                            clientState = StateTypes.Lobby;
                            currentRoom = StateTypes.Lobby;

                            username = returnUserInfo.data2;
                            inputUsername.placeholder.GetComponent<Text>().text = username;

                            loginInputUserID.text = string.Empty;
                            loginInputPassword.text = string.Empty;

                            loginPanel.SetActive(false);
                            lobbyPanel.SetActive(true);
                            AlwaysGroup.SetActive(true);
                            

                            AllRoomListRequest();
                        }
                        else 
                        {
                            print("Login Failed");
                            systemAlertText.text = returnUserInfo.data2;
                            systemAlert.SetActive(true);
                        }
                    }
                    // Register
                    else if (returnUserInfo.eventName == StateTypes.Register)
                    {
                        print("Register");
                        if (returnUserInfo.data1 == EventTypes.Success)
                        {
                            registerInputUserID.text = string.Empty;
                            registerInputUsername.text = string.Empty;
                            registerInputPassword.text = string.Empty;
                            registerInputRePassword.text = string.Empty;

                            loginPanel.SetActive(true);
                            registerPanel.SetActive(false);

                            systemAlertText.text = "Register Success";
                            systemAlert.SetActive(true);
                        }
                        else //returnUserInfo.data1 == "Failed"
                        {
                            print("Register: Failed");
                            systemAlertText.text = returnUserInfo.data2;
                            systemAlert.SetActive(true);
                        }
                    }
                    loadingPanel.SetActive(false);
                }


                tempData = string.Empty;
                loadingPanel.SetActive(false);
            }

            //เอาไว้จับเวลาเซิร์ฟ แต่ขี้เกียจแล้ว
            //else if(clientState == StateTypes.Connecting)
            //{

            //}
        }

        public void JoinRoomRequest()
        {
            if (inputRoomName.text == string.Empty)
            {
                systemAlertText.text = "Please input room name.";
                systemAlert.SetActive(true);
                return;
            }
            if (ConnectionCheck())
            {
                clientState = StateTypes.JoinRoom;
                MessageChat newMsg = new MessageChat();
                newMsg.eventName = StateTypes.JoinRoom;
                newMsg.Room = inputRoomName.text;
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

            if (ConnectionCheck())
            {
                clientState = StateTypes.CreateRoom;
                //print("Room Creating Request : " + inputRoomName.text);
                MessageChat newMsg = new MessageChat();
                newMsg.eventName = StateTypes.CreateRoom;
                newMsg.Room = inputRoomName.text;
                newMsg.Sender = EventTypes.ChatSystem;
                newMsg.data = "<color=red>" + username + "</color>" + " coming here.";
                websocket.Send(newMsg.MsgObjtoJson());
            }
        }
        public void LeaveRoom()
        {
            if (ConnectionCheck())
            {
                MessageChat newMsg = new MessageChat();
                newMsg.eventName = StateTypes.LeaveRoom;
                newMsg.Room = currentRoom;
                newMsg.Sender = EventTypes.ChatSystem;
                newMsg.data = "<color=red>" + username + "</color>" + " leave the room.";

                websocket.Send(newMsg.MsgObjtoJson());
                foreach (MessageChatBox box in MsgBoxList)
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
            if (ConnectionCheck())
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
            if (ConnectionCheck())
            {
                MessageChat newMsg = new MessageChat();
                newMsg.eventName = EventTypes.AllRoomList;
                websocket.Send(newMsg.MsgObjtoJson());
            }
        }

        public void LoginRequest()
        {
            if (loginInputUserID.text != string.Empty
                && loginInputPassword.text != string.Empty)
            {
                if (ConnectionCheck())
                {
                    loadingPanel.SetActive(true);
                    userInfo.eventName = StateTypes.Login;
                    userInfo.iD = loginInputUserID.text;
                    userInfo.data1 = loginInputPassword.text;
                    userInfo.data2 = string.Empty;
                    websocket.Send(JsonUtility.ToJson(userInfo));
                
                    print("Login Request: "+JsonUtility.ToJson(userInfo));
                }
            }
            else
            {
                systemAlertText.text = "Please input all field.";
                systemAlert.SetActive(true);
            }
        }

        public void RegisterRequest()
        {
            if (registerInputUserID.text != string.Empty
                && registerInputUsername.text != string.Empty
                && registerInputPassword.text != string.Empty
                && registerInputRePassword.text != string.Empty)
            {
                if (registerInputPassword.text != registerInputRePassword.text)
                {
                    systemAlertText.text = "Password not match";
                    systemAlert.SetActive(true);
                }
                else
                {
                    if (ConnectionCheck())
                    {
                        loadingPanel.SetActive(true);
                        userInfo.eventName = StateTypes.Register;
                        userInfo.iD = registerInputUserID.text;
                        userInfo.data1 = registerInputPassword.text;
                        userInfo.data2 = registerInputUsername.text;
                        print("Register Request: "+ JsonUtility.ToJson(userInfo));
                        websocket.Send(JsonUtility.ToJson(userInfo));
                    }
                }
            }
            else
            {
                systemAlertText.text = "Please input all field.";
                systemAlert.SetActive(true);
            }
        }

        public void WebSocketConnection()
        {
            Debug.Log($"Connect >>> ws://{iPAddress}:{port}/");

            websocket = new WebSocket($"ws://{iPAddress}:{port}/");
            websocket.OnMessage += OnMessage;
            websocket.Connect();
            if (websocket.ReadyState == WebSocketState.Open)
            {
                loginPanel.SetActive(true);
            }
            else
            {
                systemAlertText.text = "Can't connect to the server.";
                systemAlert.SetActive(true);
                connectButton.SetActive(true);
                print("Reconnect Plz"); 
            }
        }

        public void SendMSG()
        {
            if (ConnectionCheck())
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
                    newMsg.eventName = StateTypes.LeaveRoom;
                    newMsg.Room = currentRoom;
                    newMsg.Sender = EventTypes.ChatSystem;
                    newMsg.data = "<color=red>" + username + "</color>" + " leave the room.";

                    websocket.Send(newMsg.MsgObjtoJson());
                }
                websocket.Close();
            }
        }

        public void Logout()
        {
            if ((websocket != null) && (websocket.ReadyState == WebSocketState.Open))
            {
                LeaveRoom(); //ไม่ต้องกลัวจะออกทั้งๆที่ไม่ได้อยู่ในห้องแชท เพราะมีการเช็คในฟังก์ชั่นอยู่แล้ว
                
                print("Logout");

                clientState = StateTypes.Idle;
                currentRoom = string.Empty;

                //return to login panel
                loginPanel.SetActive(true);
                lobbyPanel.SetActive(false);
                chatRoomPanel.SetActive(false);
                AlwaysGroup.SetActive(false);
            }
            userInfo = new UserInfo();
            //Application.Quit();
        }

        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            tempData = messageEventArgs.Data;
        }

        private void MessageBoxBuild(MessageChat MsgChat)
        {
            print("Building MessageBox");
            print(MsgChat.MsgObjtoJson());
            MessageChatBox newMsgBox = new MessageChatBox();
            newMsgBox.msgData = MsgChat;
            GameObject newBox = Instantiate(chatBoxObject, chatBoxContent.transform);
            newMsgBox.textObject = newBox.GetComponent<Text>();

            if (MsgChat.Sender == EventTypes.ChatSystem)
            {
                newMsgBox.textObject.text = MsgChat.data;
                newMsgBox.textObject.alignment = TextAnchor.MiddleCenter;
            }
            else
            {
                if (MsgChat.Sender == username)
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

        private bool ConnectionCheck()
        {
            if ((websocket != null) && (websocket.ReadyState == WebSocketState.Open))
            {
                return true;
            }
            else
            {
                systemAlertText.text = "Connection Failed";
                systemAlert.SetActive(true);
                return false;
            }
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
    public class MessageChatBox
    {
        public MessageChat msgData;
        public Text textObject;
    }
    public static class StateTypes
    {
        public const string Idle = "Idle";
        public const string Connecting = "Connecting";
        public const string Login = "Login";
        public const string Register = "Register";
        public const string Lobby = "Lobby";
        public const string ChatRoom = "ChatRoom";
        public const string JoinRoom = "JoinRoom";
        public const string LeaveRoom = "LeaveRoom";
        public const string CreateRoom = "CreateRoom";
        public const string Disconnect = "Disconnect";
    }

    public static class EventTypes
    {
        public const string AllRoomList = "AllRoomList";
        public const string Connect = "Connect";
        public const string ChatMsg = "ChatMsg";
        public const string ChatSystem = "ChatSystem";

        public const string Request = "Request";
        public const string Success = "Success";
        public const string Failed = "Failed";

        public const string ErrorAlert = "ErrorAlert";
    }
}
