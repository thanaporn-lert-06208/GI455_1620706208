using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MultiPlayerExampleWeek10 {
    public class Bullet : MonoBehaviour
    {
        public NetworkObject netObj;
        private float countTime;
        // Start is called before the first frame update
        void Start()
        {
            netObj = GetComponent<NetworkObject>();
        }

        // Update is called once per frame
        void Update()
        {
            if(netObj.IsOwner())
            {
                this.transform.position += Vector3.up * 10.0f * Time.deltaTime;

                countTime += Time.deltaTime;

                if(countTime > 2.0f)
                {
                    SocketConnect.instance.DestroyNetworkObject(netObj.objectID);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Box")
            {
                Debug.Log("Add Score");
                GameManager.instance.AddScore(5);
                SocketConnect.instance.DestroyNetworkObject(netObj.objectID);
            }
        }
    }
}