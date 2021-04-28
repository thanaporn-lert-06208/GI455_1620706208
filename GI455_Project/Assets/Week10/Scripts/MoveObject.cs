using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//08.31

/*
 * เอาไปใส่ใน Object ที่ขยับได้เด้อ
 */
namespace MultiPlayerExampleWeek10
{

    public class MoveObject : MonoBehaviour
    {
        public float hp;
        public TextMeshPro textHP;
        public TextMeshPro textMesh;
        private NetworkObject netObj;
        public MeshRenderer mesh;
        public Transform shootingPoint;

        public GameObject bullet;
        // Start is called before the first frame update
        void Start()
        {
            netObj = GetComponent<NetworkObject>();
        }

        // Update is called once per frame
        void Update()
        {
            if (netObj.IsOwner())
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                this.transform.position += Vector3.right * Input.GetAxis("Horizental") * 3.0f * Time.deltaTime;
                if(horizontalInput > 0)
                {
                    this.transform.right = Vector3.right;
                }
                else if(horizontalInput < 0)
                {
                    this.transform.right = -Vector3.right;
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    //SocketConnect.instance.SendFunction(netObj, "Shoot", this.GetType(), null);
                    //SocketConnect.instance.SpawnNetworkObject("Bullet", shootingPoint.position, shootingPoint.rotation);
                    SocketConnect.instance.DestroyNetworkObject(netObj.objectID);
                }
                //netObj.replicateData.position = this.transform.position;
                //netObj.replicateData.rotation = this.transform.rotation;
                //netObj.replicateData.hp = hp;


            }
            else
            {
                //this.transform.position = netObj.correctPosition;
                this.transform.position = Vector3.Lerp(this.transform.position, netObj.correctPosition, 5.0f * Time.deltaTime);
                //มั่วตอนท้าย this.transform.rotation = Quaternion.Lerp(this.transform.position, netObj.replicateData.rotation, 5.0f * Time.deltaTime);
            }
            //textMesh.SetText(netObj.replicateData.hp.ToString("0"));
            //textPivot.transforn.rotation = Quaternion.identity;
        }

        public void Shoot()
        {
            //Instantiate(bulletPref, shootingPoint.position, shootingPoint.rotation);
        }
    }
}