using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupMemory : MonoBehaviour {

	public bool[] playerActive = new bool[4];
	public bool[] disableControl = new bool[4];
	public float lives;
	public SetupConfiguration setup;

	// Use this for initialization
	void Awake () {
		
		DontDestroyOnLoad(gameObject);

	}
	
	// Update is called once per frame
	public void UpdateSetup () {

	gameObject.tag = "OldSetupMemory";
	for (int i = 0; i < playerActive.Length; i++)
	{
		playerActive[i] = setup.playerActive[i];
		disableControl[i] = setup.disableControl[i];
	}
	lives = setup.lives;
	
	}
}
