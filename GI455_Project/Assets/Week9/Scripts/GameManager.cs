using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using MultiPlayerExampleWeek9;

namespace MultiPlayerExampleWeek9
{
    public class GameManager : MonoBehaviour
    {
        //Singleton คืออะไรไม่รู้ ไปหาเอาเอง

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
                if (SocketConnect.instance.currentRoom == null)
                {
                    roomName = GUILayout.TextField(roomName);

                    if (GUILayout.Button("CreateRoom"))
                    {
                        Room.RoomOption roomOption = new Room.RoomOption();
                        roomOption.roomName = roomName;
                        SocketConnect.instance.CreateRoom(roomOption);
                    }
                    else if(GUILayout.Button("JoinRoom"))
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
        }
    }
}
