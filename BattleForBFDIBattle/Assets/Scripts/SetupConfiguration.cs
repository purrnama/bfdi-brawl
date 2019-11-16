using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetupConfiguration : MonoBehaviour
{
    public bool[] playerActive = new bool[4];
    public bool[] disableControl = new bool[4];
    public float lives = 3f;
    Transform live;
    TextMeshProUGUI liveText;

    void Start(){
        live = gameObject.transform.Find("Lives");
        liveText = live.Find("LivesNumber").gameObject.GetComponent<TextMeshProUGUI>();
    }


    public void Active1(bool active){

        playerActive[0] = active;

    }
    public void Active2(bool active){

        playerActive[1] = active;

    }
    public void Active3(bool active){

        playerActive[2] = active;

    }
    public void Active4(bool active){

        playerActive[3] = active;

    }
    public void Disable1(bool active){

        disableControl[0] = active;

    }
    public void Disable2(bool active){

        disableControl[1] = active;

    }
    public void Disable3(bool active){

        disableControl[2] = active;

    }
    public void Disable4(bool active){

        disableControl[3] = active;

    }
    public void Lives(float index){

        lives = index;
        liveText.text = lives.ToString();

    }
}
