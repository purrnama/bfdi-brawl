using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPanelController : MonoBehaviour
{
    Animator anim;
    Transform parent;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        parent = gameObject.transform.parent;
        SplashScreenController splash = parent.GetComponent<SplashScreenController>();
        splash.OnShowInfoPanel += ShowInfoPanel;
        splash.OnDismissInfoPanel += DismissInfoPanel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowInfoPanel(){
        anim.SetTrigger("Show");
    }
    void DismissInfoPanel(){
        anim.SetTrigger("Dismiss");
    }
}
