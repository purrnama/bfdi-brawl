using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    PlayerMovement targetPlayer = null;
    private Vector3 refVelocity;
    // Start is called before the first frame update

    // Update is called once per frame
    void LateUpdate()
    {
        if(targetPlayer != null){
            transform.position = Vector3.SmoothDamp(transform.position, targetPlayer.transform.position, ref refVelocity, 0.05f);
        }
    }
}
