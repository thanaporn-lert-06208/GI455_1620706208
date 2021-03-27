using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * เอาไปใส่ใน Object ที่ขยับได้เด้อ
 */
namespace MultiPlayerExampleWeek9
{
    
    public class MoveObject : MonoBehaviour
    {
        private NetworkObject netObj;
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            this.transform.position += Vector3.right * Input.GetAxis("Horizental") * Time.deltaTime;
        }
    }
}