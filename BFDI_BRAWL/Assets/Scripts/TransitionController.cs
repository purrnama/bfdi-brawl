using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    Animator anim;
    Transform parent;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        parent = gameObject.transform.parent;
        SplashScreenController splash = parent.GetComponent<SplashScreenController>();
        if (splash != null){
            splash.OnStartInitialize += StartTransition;
            splash.OnFinishInitialize += EndTransition;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        EndTransition();
    }

    void StartTransition(){
        anim.SetTrigger("Start");
    }
    void EndTransition(){
        anim.SetTrigger("End");
    }
}
