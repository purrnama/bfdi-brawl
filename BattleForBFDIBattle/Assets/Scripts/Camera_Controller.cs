using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Camera_Controller : MonoBehaviour {

    public Camera cam;
	public List<Transform> targets;
	public Vector3 offset;
	public float smoothTime, maxZoom, minZoom, zoomLimiter;

	[Header("Camera Bounds")]
	public bool boundDisable;
	public float BoundsUp;
	public float BoundsDown, BoundsLeft, BoundsRight;

	private Vector3 velocity;

	[Header("Shake Parameters")]

	public float lightMagnitude;
	public float heavyMagnitude, deathMagnitude, roughness, fadeIn, fadeOut;

	void Start(){

	}

	void FixedUpdate(){

		if (targets.Count == 0) {
			return;
		}
		Move ();
		Zoom ();
	
	}
	void Zoom(){

		float newZoom = Mathf.Lerp (maxZoom, minZoom, GetGreatestDistance ()/ zoomLimiter);
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
	}

	void Move(){

		Vector3 centerPoint = GetCenterPoint();
		Vector3 newPosition = centerPoint + offset;
		transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);

		if(!boundDisable){
		if(transform.position.x > BoundsRight){
			transform.position = new Vector3(BoundsRight, transform.position.y, transform.position.z);
		}
		if(transform.position.x < BoundsLeft){
			transform.position = new Vector3(BoundsLeft, transform.position.y, transform.position.z);
		}
		if(transform.position.y < BoundsDown){
			transform.position = new Vector3(transform.position.x, BoundsDown, transform.position.z);
		}
		if(transform.position.y > BoundsUp){
			transform.position = new Vector3(transform.position.x, BoundsUp, transform.position.z);
		}
		}

	}

	float GetGreatestDistance(){

		var bounds = new Bounds (targets [0].position, Vector3.zero);
		for (int i = 0; i < targets.Count; i++) {

			bounds.Encapsulate (targets [i].position);
		
		}

		float selectBigger = 0f;

		if(bounds.size.x > bounds.size.y){
			selectBigger = bounds.size.x;
		}else{
			selectBigger = bounds.size.y;
		}

		return selectBigger;

	}

	Vector3 GetCenterPoint(){

		if (targets.Count == 1) {
			return targets [0].position;
		}

		var bounds = new Bounds (targets [0].position, Vector3.zero);
		for (int i = 0; i < targets.Count; i++) {

			bounds.Encapsulate (targets [i].position);

		}
		return bounds.center;
	}
	public void ZoomPunch(){

		transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + 1f);

	}
	public void ZoomSpecial(){

		transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + 3f);

	}

	public void SmolShake(){

		CameraShaker.GetInstance("Cam").ShakeOnce(lightMagnitude, roughness, fadeIn, fadeOut);

	}
	
	public void BigShaq(){

		CameraShaker.GetInstance("Cam").ShakeOnce(heavyMagnitude, roughness, fadeIn, fadeOut);

	}

	public void DeathShake(){

		CameraShaker.GetInstance("Cam").ShakeOnce(deathMagnitude, roughness, fadeIn, fadeOut);

	}
}
