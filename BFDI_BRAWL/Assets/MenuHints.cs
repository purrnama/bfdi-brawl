using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuHints : MonoBehaviour
{
    
    TextMeshProUGUI text;
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        anim = GetComponent<Animator>();
    }

    public void HoverHint(string hint){
        text.text = hint;
        anim.SetTrigger("HintPrompt");
    }
}
