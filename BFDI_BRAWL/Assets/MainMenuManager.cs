using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] TransitionController transition;
    [SerializeField] private GameObject main, settings, backButton;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void OpenSettings(){
        StartCoroutine(OpenSettingsCoroutine());
    }

    public void Back(){
        StartCoroutine(BackCoroutine());
    }

    IEnumerator OpenSettingsCoroutine(){
        if(settings.activeSelf == false){
            transition.StartTransition();
            yield return new WaitForSeconds(0.4f);
            main.SetActive(false);
            settings.SetActive(true);
            backButton.SetActive(true);
            transition.EndTransition();
        }
    }

    IEnumerator BackCoroutine(){
        transition.StartTransition();
        yield return new WaitForSeconds(0.4f);
        main.SetActive(true);
        settings.SetActive(false);
        backButton.SetActive(false);
        transition.EndTransition();
    }
    public void ExitGame(){
        //TODO: Should add a confirmation prompt
        Application.Quit();
    }
}
