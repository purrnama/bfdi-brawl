using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformEdge : MonoBehaviour
{
    public Vector3 edgePoint = Vector3.zero;
    // Start is called before the first frame update
    private void OnDrawGizmosSelected() {
        Gizmos.DrawSphere((transform.position + edgePoint), 0.1f);
    }
}
