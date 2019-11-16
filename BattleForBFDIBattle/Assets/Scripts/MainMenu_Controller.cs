using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu_Controller : MonoBehaviour {

	GameObject canvas;
	GameObject transition;
	GameObject cam;
	GameObject changelogPanel;
	GameObject logo;
	GameObject betaSetup;
	GameObject battleButton;
	GameObject optionsButton;
	GameObject creditsButton;
	GameObject exitButton;
	GameObject maindescriptionText;
	GameObject pressanybuttonText;
	SetupMemory setupMem;
	bool startup = true;

	void Awake(){

		canvas = GameObject.Find("Canvas");
		transition = GameObject.FindGameObjectWithTag("Transition");
		cam = GameObject.Find("Main Camera");
		betaSetup = GameObject.FindGameObjectWithTag("Setup");
		setupMem = GameObject.FindGameObjectWithTag("SetupMemory").GetComponent<SetupMemory>();
		changelogPanel = canvas.transform.Find("ChangelogPanel").gameObject;
		logo = canvas.transform.Find("Logo").gameObject;
		battleButton = canvas.transform.Find("BattleButton").gameObject;
		optionsButton = canvas.transform.Find("OptionsButton").gameObject;
		creditsButton = canvas.transform.Find("CreditsButton").gameObject;
		exitButton = canvas.transform.Find("ExitButton").gameObject;
		pressanybuttonText = canvas.transform.Find("PressAnyButton").gameObject;
		maindescriptionText = canvas.transform.Find("MainDescription").gameObject;
		setupMem.setup = betaSetup.GetComponent<SetupConfiguration>();
		GameObject oldSetupMem = GameObject.FindGameObjectWithTag("OldSetupMemory");
		if(oldSetupMem != null){
			Destroy(oldSetupMem);
		}
		
		betaSetup.SetActive(false);
		battleButton.SetActive(false);
		optionsButton.SetActive(false);
		creditsButton.SetActive(false);
		exitButton.SetActive(false);
		changelogPanel.SetActive(false);
		pressanybuttonText.SetActive(true);
		maindescriptionText.SetActive(false);


	}
	void Update(){
		if(Input.anyKeyDown && startup){
			startup = false;
			logo.GetComponent<Animator>().SetTrigger("Startup->Menu");
			pressanybuttonText.SetActive(false);
			battleButton.SetActive(true);
			optionsButton.SetActive(true);
			creditsButton.SetActive(true);
			exitButton.SetActive(true);
			changelogPanel.SetActive(true);
			maindescriptionText.SetActive(true);
		}
	}
	public void BetaSetup(){
		changelogPanel.GetComponent<Animator>().SetTrigger("End");
		logo.GetComponent<Animator>().SetTrigger("End");
		betaSetup.SetActive(true);
		battleButton.SetActive(false);
	}

	public void LoadScene(string sceneName){

		cam.GetComponent<Animator>().SetTrigger("GameView");
		setupMem.UpdateSetup();
		transition.GetComponent<Transition_Controller>().LoadSceneOnTransition(sceneName);

	}
}
