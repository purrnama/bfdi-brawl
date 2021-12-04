using System.Collections;
using System.Collections.Generic;
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
            splash.OnStartInitialize += StartTranstition;
            splash.OnFinishInitialize += EndTranstition;
        }
    }
    void StartTranstition(){
        anim.SetTrigger("Start");
    }
    void EndTranstition(){
        anim.SetTrigger("End");
    }
}
