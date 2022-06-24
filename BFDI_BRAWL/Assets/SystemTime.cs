using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SystemTime : MonoBehaviour
{

    TextMeshProUGUI text;
    string time;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        time = System.DateTime.Now.ToString("HH:mm");
        text.text = time;
    }
}
