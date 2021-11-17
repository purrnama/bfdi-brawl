using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Battle Scene", menuName = "BFBB/Battle Scene ID")]
public class BattleSceneID : ScriptableObject
{
    public string sceneName = "Untitled Battle Scene";
    public GameObject asset = null;

    public Vector3[] playerPosition = new Vector3[3];
    public AudioClip music = null;
    public Material skybox = null;
    public Color fogColor = new Color(0f, 0f, 0f, 255f); 

    [Header("Arena Bounds")]
    public Vector3 stageCenter = Vector3.zero;
    public float boundsX = 0f;
    public float boundsY = 0f;
    public float boundsZ = 0f;
    
}
