using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleKey_Test : MonoBehaviour {
	int keyA = 0;

	void Update () {

	 if(Input.GetKeyDown(KeyCode.A)){ //We pressed key A
     	if(keyA ==0)keyA = 1; //If keyA is 0 then set it to one
     	else if(keyA == 1)keyA = 2; //If keyA is 1 then set it to two
     }

     else if(keyA==2){

		 Debug.Log("Double Tap");
     	keyA =0;
     }

     else{
     //This mean there is only 1 key pressed or no key is pressed
     //You need to add time here, after 1 sec keyA will be again 0
     }


	}
}
