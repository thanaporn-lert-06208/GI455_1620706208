using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using MultiPlayerExampleWeek9;

namespace MultiPlayerExampleWeek10
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        public class TestSendOnce
        {
            public int hp;
        }

        //Singleton คืออะไรไม่รู้ ไปหาเอาเอง

        public string roomName;

        public int currentScore;

        public static GameManager instance;

        public void Awake()
        {
            instance = this;
        }
        public void OnGUI()
        {
            if (SocketConnect.instance.IsConnected() == false)
            {
                if (GUILayout.Button("Connect"))
                {
                    SocketConnect.instance.Connect();
                }
            }
            else
            {
                if (SocketConnect.instance.currentRoom == null)
                {
                    roomName = GUILayout.TextField(roomName);

                    if (GUILayout.Button("CreateRoom"))
                    {
                        Room.RoomOption roomOption = new Room.RoomOption();
                        roomOption.roomName = roomName;
                        SocketConnect.instance.CreateRoom(roomOption);
                    }
                    else if (GUILayout.Button("JoinRoom"))
                    {
                        Room.RoomOption roomOption = new Room.RoomOption();
                        roomOption.roomName = roomName;
                        SocketConnect.instance.CreateRoom(roomOption);
                    }
                }
                else
                {
                    if (GUILayout.Button("SpawnNetworkObject"))
                    {
                        SocketConnect.instance.SpawnNetworkObject("Sphere", Vector3.zero, Quaternion.identity);
                    }
                }
            }
            
            if(GUILayout.Button("TestCallFunction"))
            {

            }
        }
        
        public void AddScore(int addedScore)
        {
            currentScore += addedScore;
        }

    }

    

    
}
