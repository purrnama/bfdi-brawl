using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingController : MonoBehaviour
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
            splash.OnStartInitialize += ShowLoading;
            splash.OnFinishInitialize += EndLoading;
        }
    }
    void ShowLoading(){
        anim.SetTrigger("Show");
    }
    void EndLoading(){
        anim.SetTrigger("End");
    }
}
