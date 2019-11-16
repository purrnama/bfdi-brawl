using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerIndicator_Controller : MonoBehaviour {
	
    public Transform playerRef;
	TextMeshPro text;
	Vector3 currentVelocity;

	// Use this for initialization
	void Start () {
		
		text = transform.Find("Title").gameObject.GetComponent<TextMeshPro>();
		text.text = playerRef.gameObject.name;

	}
	
	// Update is called once per frame
}
