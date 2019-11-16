using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[CreateAssetMenu(fileName = "New Battle Scene", menuName = "BFBB/Battle Scene ID")]
public class BattleSceneID : ScriptableObject
{
    public string sceneName = "Untitled Battle Scene";
    public GameObject asset = null;

    public Vector3[] playerPosition = new Vector3[3];
    public float gravity = -36;
    public AudioClip music = null;
    public PostProcessProfile postProcessing = null;
    public Material skybox = null;

    [Header("Arena Bounds")]
    public float boundsUp = 0f;
    public float boundsDown = 0f;
    public float boundsLeft = 0f;
    public float boundsRight = 0f;
    
}
