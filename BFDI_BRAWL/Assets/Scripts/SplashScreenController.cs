using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class SplashScreenController : MonoBehaviour
{
    
    internal bool anyKeyPressed = false;
    [SerializeField] float delayLoad = 0f;
    AudioSource audioSource;
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioClip infoSound;
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
        yield return new WaitForSeconds(2f);
        OnShowInfoPanel();
        audioSource.PlayOneShot(infoSound);
        yield return new WaitForSeconds(5f);
        OnDismissInfoPanel();
    }
}
