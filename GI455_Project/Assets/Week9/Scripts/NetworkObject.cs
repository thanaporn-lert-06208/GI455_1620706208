using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiPlayerExampleWeek9
{
    public class NetworkObject : MonoBehaviour
    {
        public string ownerID;
        public string objectID;

        public Vector3 correctPosition;
        public Quaternion correctRotation;

        public bool IsOwner()
        {
            return SocketConnect.instance.clientID;
        }
    }
}