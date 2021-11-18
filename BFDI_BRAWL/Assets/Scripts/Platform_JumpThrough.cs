using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform_JumpThrough : MonoBehaviour
{
    int layer;

    void Start() {
        layer = gameObject.layer;
    }
    void OnTriggerEnter(Collider c)
    {
        Physics.IgnoreLayerCollision(layer, c.gameObject.layer, false);
        Debug.Log(Physics.GetIgnoreLayerCollision(layer, c.gameObject.layer));
    }
    
    void OnTriggerExit(Collider c)
    {
        Physics.IgnoreLayerCollision(layer, c.gameObject.layer, true);
        Debug.Log(Physics.GetIgnoreLayerCollision(layer, c.gameObject.layer));
    }
}
