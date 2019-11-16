using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow_Player : MonoBehaviour
{
    
	public Transform playerToFollow;
	public Vector3 offset;
	public float smoothTime;
	Vector3 currentVelocity;
	void FixedUpdate () {
		
		if(playerToFollow == null){

			Debug.Log("No player selected on Indicator you DUMMY!");
			return;
	
		}

		Vector3 newPosition = new Vector3(playerToFollow.position.x + offset.x, playerToFollow.position.y + offset.y, playerToFollow.position.z + offset.z);
		transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref currentVelocity , smoothTime);

	}
}
