using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class NewBehaviourScript : MonoBehaviour
    {
        public void Start()
        {
            string Wow = JsonUtility.ToJson(new Message());
            print(Wow);
            
        }
    }

    public class Message
    {
        public string EventType;
        public string Data;

        public Message()
        {
            EventType = "System";
        }
    }
}