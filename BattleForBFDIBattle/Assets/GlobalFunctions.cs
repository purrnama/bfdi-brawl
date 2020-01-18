using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalFunctions : MonoBehaviour
{
    bool hitStopped;
    public void HitStop(float time){
        if(!hitStopped){   
            Time.timeScale = 0.0f;
        }else{
            StartCoroutine(Wait(time));
        }
    }

    IEnumerator Wait(float duration){
        hitStopped = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
        hitStopped = false;
    }
}
