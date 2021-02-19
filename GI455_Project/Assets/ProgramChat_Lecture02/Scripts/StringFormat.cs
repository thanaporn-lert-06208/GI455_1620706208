using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProgramChat
{
    public class StringFormat : MonoBehaviour
    {
        public class MessageData
        {
            public string username;
            public string message;
            public string colorName;
        }
        // Start is called before the first frame update
        void Start()
        {
            string reveiveMessage = "inwza007#hello world";
            string[] messageDataSplit = reveiveMessage.Split('#');

            MessageData messageData = new MessageData();
            messageData.username = messageDataSplit[0];
            messageData.message = messageDataSplit[1];
            messageData.colorName = messageDataSplit[2];

            if (messageDataSplit[0] == "inwza007")
            {
                ShowMessage(messageData.message);
            }

        }



        void ShowMessage(string message)
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
