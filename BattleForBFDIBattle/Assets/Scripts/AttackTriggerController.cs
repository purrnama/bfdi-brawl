using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTriggerController : MonoBehaviour {

	GameObject player;
	GameObject parent;
	Player_Controller playerControl;
	BoxCollider collide;
	Vector3 colliderPosition;
	int combo;
	float comboTime;
	public float comboSetTime;

	public bool flipped;
	public bool heavy;

	Transform effectStash;
	GameObject lightSmash, heavySmash;


	// Use this for initialization
	void Start () {
		
		player = transform.parent.gameObject;
		parent = GameObject.Find("GameObjects");
		playerControl = player.GetComponent<Player_Controller>();
		collide = GetComponent<BoxCollider>();
		colliderPosition = playerControl.characterID.triggerCenter;
		collide.center = colliderPosition;
		collide.size = playerControl.characterID.triggerSize;
		effectStash = parent.transform.Find("EffectsStash").transform;
		lightSmash = parent.transform.Find("LightSmash").gameObject;
		heavySmash = parent.transform.Find("HeavySmash").gameObject;

	}
	
	// Update is called once per frame
	void Update () {

		flipped = playerControl.flipped;
		if(flipped){

			collide.center = new Vector3(-colliderPosition.x,colliderPosition.y, colliderPosition.z);

		}else{

			collide.center = new Vector3(colliderPosition.x, colliderPosition.y, colliderPosition.z);

		}

	}

	void OnTriggerEnter(Collider c){

		Vector3 spawnPos = player.transform.position + collide.center;

		if(c.gameObject.tag == "Player"){

			if(heavy){
				GameObject HeavySmash = Instantiate(heavySmash, spawnPos, Quaternion.identity, effectStash);
				HeavySmash.SetActive(true);
			}else{
				GameObject LightSmash = Instantiate(lightSmash, spawnPos, Quaternion.identity, effectStash);
				LightSmash.SetActive(true);
			}

		}

	}
}
