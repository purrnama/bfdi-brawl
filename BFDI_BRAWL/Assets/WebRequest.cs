using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(GetText());
    }
    public IEnumerator GetText(){
        string update;
        UnityWebRequest www = UnityWebRequest.Get("https://smolpadok.github.io/bfdi-brawl/update.txt");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success){
            Debug.LogWarning(www.error);
        }
        else{
            update = www.downloadHandler.text;
        }
    }
}
