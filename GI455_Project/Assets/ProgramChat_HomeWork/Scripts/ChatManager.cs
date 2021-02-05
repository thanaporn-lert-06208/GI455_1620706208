using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace ProgramChat_Homework
{
    public class ChatManager : MonoBehaviour
    {
        private WebSocket websocket;

        //Log in Panel
        public Text iPAddressTitle;
        public Text inputIPAddress;

        public Text portTitle;
        public Text inputPort;

        private string iPAddress;
        private string port;
        private string username;

        public Text userNameTitle;
        public Text inputUsername;

        //Chat Panel

        public GameObject connectButton;
        public GameObject changeUsernameButton;

        public InputField inputChat;
        public GameObject chatBoxObject;
        public GameObject chatBoxContent;

        private string cutHerePls = "|| cutherepls ||";
        private List<Message> messagesList = new List<Message>();

        void Start()
        {
            iPAddress = "127.0.0.1";
            port = "5500";
            username = "Annonymous";
    }

        private void Update()
        {

        }

        public void WebSocketConnection()
        {
            if (inputUsername.text != string.Empty)
            {
                username = inputUsername.text;
                userNameTitle.text = "Username: " + username;
                inputUsername.text = string.Empty;
            }
            if (inputIPAddress.text != string.Empty)
            {
                iPAddress = inputIPAddress.text;
                iPAddressTitle.text = "IP Address: " + iPAddress;
                inputIPAddress.text = string.Empty;
            }
            if (inputPort.text != string.Empty)
            {
                port = inputPort.text;
                portTitle.text = "Port: " + iPAddress;
                inputPort.text = string.Empty;
            }
            MessageBoxBuild("SystemMessage" + cutHerePls + "<color=orange>Joining</color>");
            MessageBoxBuild("SystemMessage" + cutHerePls + "<color=orange>...</color>");

            Debug.Log($"ws://{iPAddress}:{port}/");

            websocket = new WebSocket($"ws://{iPAddress}:{port}/");
            websocket.OnMessage += OnMessage;
            websocket.Connect();
            if (websocket.ReadyState == WebSocketState.Open)
            {
                websocket.Send("SystemMessage" + cutHerePls + "<color=red>" + username + "</color>" + " coming here.");
            }
        }

        public void SendMSG()
        {
            if ((websocket.ReadyState == WebSocketState.Open)&&(inputChat.text != string.Empty))
            {
                websocket.Send(username + cutHerePls + ": " + inputChat.text);
                inputChat.text = string.Empty;
            }
        }

        private void OnDestroy()
        {
            if (websocket != null)
            {
                if (websocket.ReadyState == WebSocketState.Open)
                {
                    websocket.Send("SystemMessage" + cutHerePls + "<color=red>" + username + "</color>" + " leave ChatBox.");
                }
                websocket.Close();
            }
        }

        public void DisConnection()
        {
            if ((websocket != null)&&(websocket.ReadyState == WebSocketState.Open))
            {
                MessageBoxBuild("SystemMessage" + cutHerePls + "<color=orange>Leave</color>");
                websocket.Send("SystemMessage" + cutHerePls + "<color=red>" + username + "</color>" + " leave ChatBox.");
                websocket.Close();
            }
            //Application.Quit();
        }

        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            MessageBoxBuild(messageEventArgs.Data);
        }

        private void MessageBoxBuild(string recieveText)
        {
            string[] cutText = recieveText.Split(new string[] { cutHerePls }, System.StringSplitOptions.None);

            Message newMessage = new Message();
            GameObject newChatBox = Instantiate(chatBoxObject, chatBoxContent.transform);
            newMessage.textObject = newChatBox.GetComponent<Text>();

            if (cutText.Length != 2)
            {
                cutText = new string[] { "SystemMessage", "Error content" };
            }

            if (cutText[0] == "SystemMessage")
            {
                newMessage.textObject.alignment = TextAnchor.MiddleCenter;
                newMessage.Text = cutText[1];
                newMessage.textObject.text = cutText[1];
            }
            else
            {
                if (cutText[0] == username)
                {
                    newMessage.textObject.alignment = TextAnchor.MiddleRight;
                    newMessage.Text = "<color=green>" + cutText[0] + "</color>" + cutText[1];
                }
                else
                {
                    newMessage.textObject.alignment = TextAnchor.MiddleLeft;
                    newMessage.Text = "<color=darkblue>" + cutText[0] + "</color>" + cutText[1];
                }
                newMessage.textObject.text = newMessage.Text;
            }
            
            var boxHeight = newMessage.textObject.GetComponent<RectTransform>().rect.height;
            foreach (Message msg in messagesList)
            {
                Transform rt = msg.textObject.GetComponent<Transform>();
                rt.position = rt.position + new Vector3(0, 3f, 0);
            }
            messagesList.Add(newMessage);
        }

        public void ChangeUsername()
        {
            if((inputUsername.text != string.Empty)&&(websocket.ReadyState == WebSocketState.Open))
            {
                websocket.Send("SystemMessage" + cutHerePls + "<color=red>" + username + "</color> changed username >> " + "<color=red>" + inputUsername.text + "</color>");
                username = inputUsername.text;
                userNameTitle.text = "Username: " + username;
                inputUsername.text = "";
            }
        }
    }

    public class Message
    {
        public string Text;
        public Text textObject;
    }
    
}
