using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    float Log(float value) {
        return  Mathf.Log10 (value) * 20;
    }

    public void SetSFXVolume(float sliderValue){
        mixer.SetFloat("SFXVol", Log(sliderValue));
    }
    public void SetMusicVolume(float sliderValue){
        mixer.SetFloat("MusicVol", Log(sliderValue));
    }
}
