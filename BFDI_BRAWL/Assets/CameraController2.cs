using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController2 : MonoBehaviour
{
    [SerializeField] Vector3 offset = Vector3.zero;
    [SerializeField] float smoothTime = 0f;
    [SerializeField] Game_Manager manager = null;
    // Start is called before the first frame update
    Vector3 camCenter = Vector3.zero;
    Vector2 camTargetDistance = Vector2.zero;
    private Vector3 currentVelocity;
    private Camera cam;
    Bounds stagebounds;
    [SerializeField] float maxZoomFOV = 0f;
    [SerializeField] float minZoomFOV = 0f;
    [SerializeField] float maxZoomOrt = 0f;
    [SerializeField] float minZoomOrt = 0f;
    [SerializeField] float zoomInSpeed = 0f;
    [SerializeField] float zoomOutSpeed = 0f;

    void Start(){
        cam = GetComponent<Camera>();
    }
    void Update()
    {
        Move();
        Zoom();
    }
    void Move(){
        camCenter = manager.camCenter;
        Vector3 currentPosition = transform.position;
        Vector3 newPosition = camCenter + offset;
        transform.position = Vector3.SmoothDamp(currentPosition, newPosition, ref currentVelocity, smoothTime);
    }
    void Zoom(){
        float currentZoomFOV = cam.fieldOfView;
        float currentZoomOrt = cam.orthographicSize;
        stagebounds = manager.stagebounds;
        camTargetDistance = manager.camTargetDistance;
        float lerpX = (camTargetDistance.x / stagebounds.extents.x);
        float lerpY = (camTargetDistance.y / stagebounds.extents.y);
        lerpX = Mathf.Clamp(lerpX, 0f, 1f);
        lerpY = Mathf.Clamp(lerpY, 0f, 1f);
        float highestLerp;
        if(lerpX > lerpY){
            highestLerp = lerpX;
        }else{
            highestLerp = lerpY;
        }
        if(cam.orthographic){
            float newZoom = Mathf.Lerp(maxZoomOrt, minZoomOrt, highestLerp);
            if(currentZoomOrt > newZoom){
                cam.orthographicSize = Mathf.MoveTowards(currentZoomOrt, newZoom, zoomInSpeed/2 * Time.deltaTime);
            }else{
                cam.orthographicSize = Mathf.MoveTowards(currentZoomOrt, newZoom, zoomOutSpeed * Time.deltaTime);
            }
        }else{
            float newZoom = Mathf.Lerp(maxZoomFOV, minZoomFOV, highestLerp);
            if(currentZoomFOV > newZoom){
                cam.fieldOfView = Mathf.MoveTowards(currentZoomFOV, newZoom, zoomInSpeed * Time.deltaTime);
            }else{
                cam.fieldOfView = Mathf.MoveTowards(currentZoomFOV, newZoom, zoomOutSpeed * Time.deltaTime);
            }
        }
    }
}
