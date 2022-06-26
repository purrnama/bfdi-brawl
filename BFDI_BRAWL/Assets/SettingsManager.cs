using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{

    [SerializeField] private GameObject audioPanel, controlsPanel;

    public void OpenAudioSettings(){
        audioPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void OpenControlsSettigns(){
        audioPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }
}
