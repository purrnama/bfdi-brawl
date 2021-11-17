using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    internal PlayerMovement owner = null;
    internal float gravity = 0f;
    internal Vector3 verticalVelocity = Vector3.zero;
    internal Vector3 horizontalVelocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        verticalVelocity.y += gravity * Time.deltaTime;
        transform.position = new Vector3(transform.position.x + (horizontalVelocity.x * Time.deltaTime), transform.position.y + (verticalVelocity.y * Time.deltaTime), transform.position.z);
    }

    void OnCollisionEnter(Collision c){
        if(c.gameObject.layer == 10){
            Destroy(gameObject);
        }
    }
}
