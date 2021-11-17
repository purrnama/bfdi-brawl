using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapFramerate : MonoBehaviour
{
    [SerializeField] int framerate = 0;
    void Start()
    {
        Application.targetFrameRate = framerate;
    }
}
