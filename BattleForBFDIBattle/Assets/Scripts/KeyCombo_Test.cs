using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCombo_Test : MonoBehaviour {

	public KeyCode[] combo;
	int currentIndex = 0;
	float comboTime = 0.2f;
	float time;
	bool waitRelease;
	
	// Update is called once per frame
	void Update () {
		
		if(!waitRelease){
		if(currentIndex < combo.Length){
			if(Input.GetKeyDown(combo[currentIndex])){
				currentIndex++;
			}
		}
		else{
			Debug.Log("Uppercut");
			
			currentIndex = 0;
			time = 0f;

			waitRelease = true;
		}

		if(currentIndex > 0){
			time += Time.deltaTime;
		}
		}

		if(time > comboTime){

			time = 0f;
			currentIndex = 0;
			waitRelease = true;

		}

		for (int i = 0; i < combo.Length; i++){

			if(Input.GetKey(combo[i])){

				return;

			} else{

				waitRelease = false;

			}
			
		}

	}
}
