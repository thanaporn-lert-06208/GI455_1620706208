using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProgramChat
{
    public class JsonFormat : MonoBehaviour
    {
        class MessageJsonData
        {
            public string username;
            public string message;
            public string color;
        }
        // Start is called before the first frame update
        void Start()
        {
            string jsonStr = "{\"username\":\"inwza007\",\"message\":\"ioioio\",\"color\":\"red\"}";

            MessageJsonData messageJsonData = JsonUtility.FromJson<MessageJsonData>(jsonStr);

            Debug.Log(messageJsonData.username);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


    //"inwza007#mario"

    //{
    //    "username":"inwza007",
    //    "message":"mario",
    //}
