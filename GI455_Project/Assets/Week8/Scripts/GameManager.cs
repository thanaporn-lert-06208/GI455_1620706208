using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiPlayerExampleWeek8
{
    public class GameManager : MonoBehaviour
    {
        //Singleton

        public string roomName;
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
                if(SocketConnect.instance.currentRoom == null)
                {
                    roomName = GUILayout.TextField(roomName);

                    if (GUILayout.Button("CreateRoom"))
                    {
                        Room.RoomOption roomOption = new Room.RoomOption();
                        roomOption.roomName = roomName;
                        SocketConnect.instance.CreateRoom(roomOption);
                    }
                }
            }
        }
    }
}
