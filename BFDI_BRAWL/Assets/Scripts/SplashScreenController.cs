using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class SplashScreenController : MonoBehaviour
{
    
    internal bool anyKeyPressed = false;
    [SerializeField] float delayLoad = 0f;
    AudioSource audioSource;
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioClip infoSound;
    [SerializeField] TextMeshProUGUI infoTitle, infoDesc;
    public event Action OnStartInitialize; 
    public event Action OnFinishInitialize;
    public event Action OnShowInfoPanel; 
    public event Action OnDismissInfoPanel; 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(Keyboard.current.anyKey.isPressed){
            if(!anyKeyPressed){
                Debug.Log("Any key pressed.");
                if(clickSound != null){
                    audioSource.PlayOneShot(clickSound);
                }else{
                    Debug.LogWarning("No clickSound added.");
                }
                StartCoroutine(InitializeMenu());
                anyKeyPressed = true;
            }
        }
    }
    IEnumerator InitializeMenu(){
        yield return new WaitForSeconds(delayLoad);
        Debug.Log("Initializing menu.");
        OnStartInitialize();

        CoroutineWithData cd = new CoroutineWithData(this, RequestUpdateLog());
        yield return cd.coroutine;

        string result = cd.result.ToString();
        string title, description;
        if(result != "error"){
            var data = result.Split('|');
            title = data[0].Trim();
            description = data[1].Trim();
        }else{
            title = "Connection Error";
            description = "Failed to fetch update log from server. Please check your connection.";
        }

        infoTitle.text = title;
        infoDesc.text = description;

        yield return new WaitForSeconds(2.0f);

        OnShowInfoPanel();
        audioSource.PlayOneShot(infoSound);
    }
    public void LoadMainMenu(){
        StartCoroutine(MainMenuCoroutine());
    }

    IEnumerator MainMenuCoroutine(){
        yield return new WaitForSeconds(delayLoad);
        SceneManager.LoadSceneAsync("MainMenu");
    }
    IEnumerator RequestUpdateLog(){
        string result;
        UnityWebRequest www = UnityWebRequest.Get("https://smolpadok.github.io/bfdi-brawl/update.txt");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success){
            Debug.LogWarning(www.error);
            result = "error";
        }
        else{
            result = www.downloadHandler.text;
        }
        yield return result;
    }

}
