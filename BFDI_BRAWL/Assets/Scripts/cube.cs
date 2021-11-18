using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour
{
    public void Activate(){
        transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
    }
}
