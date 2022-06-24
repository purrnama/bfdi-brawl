using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InfoPanelController : MonoBehaviour
{
    Animator anim;
    Transform parent;
    bool isActive = false;

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
    void ShowInfoPanel(){
        anim.SetTrigger("Show");
    }
    void DismissInfoPanel(){
        anim.SetTrigger("Dismiss");
    }

    public void OpenBrowserUpdate(){
        Application.OpenURL("https://smolpadok.github.io/bfdi-brawl");
    }
    public void DismissButton(){
        DismissInfoPanel();
    }
}
