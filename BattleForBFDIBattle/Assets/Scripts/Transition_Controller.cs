using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition_Controller : MonoBehaviour {

	public void LoadSceneOnTransition(string sceneName){

		StartCoroutine(Load(sceneName));

	}

	IEnumerator Load(string name){

		gameObject.GetComponent<Animator>().SetTrigger("Transition");
		
		yield return new WaitForSeconds(1f);

		SceneManager.LoadSceneAsync(name);

	}
}
