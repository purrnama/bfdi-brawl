using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class Game_Manager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] ParticlePool particlePool;
    [SerializeField] MenuInputAction inputAction;
    [SerializeField] InputActionAsset playerAction;
    [SerializeField] bool debug_prepareScene = false;
    [SerializeField] bool debug_drawBounds = true;
    [SerializeField] private AudioSource music = null;
    [SerializeField] internal Transform focus = null;
    [SerializeField] private int roundNumber = 0;
    [SerializeField] private float roundTime = 0f;
    [SerializeField] private Transform[] indicators;
    [SerializeField] private TextMeshProUGUI[] percentages;
    [SerializeField] bool[] active;

    [Header ("Player Data")]
    [SerializeField] Character_ID[] player_ID;

    [Header("Stage Data")]
    [SerializeField] internal BattleSceneID id;
    [SerializeField] List<PlayerMovement> currentPlayers = new List<PlayerMovement>();
    List<Transform> currentIndicators = new List<Transform>();
    List<TextMeshProUGUI> currentPercentages = new List<TextMeshProUGUI>();
    internal Bounds stagebounds, blastbounds, cambounds, playerbounds;
    internal Vector3 camCenter = Vector3.zero;
    internal Vector2 playerDistance = Vector2.zero;
    internal Vector2 camTargetDistance = Vector2.zero;
    [SerializeField] internal float blastLineRange = 0;
    [SerializeField] GameObject controlPanel;

    // Start is called before the first frame update
    void Start()
    {
        if(debug_prepareScene){
            PrepareGame();
        }
    }
    void Awake(){
        inputAction = new MenuInputAction();
        inputAction.Menu.ControlPanel.performed += ctx => ControlPanel();
    }
    // Update is called once per frame
    void Update()
    {
        roundTime += Time.deltaTime;
        if(currentPlayers.Count > 0){
            CalculateBounds();
        }
        /*
        UpdateIndicators();
        for (int i = 0; i < currentPlayers.Count; i++){
            CheckBlast(i);
            UpdateUI(i);
        }
        */
    }
    void ControlPanel(){
        if(controlPanel.activeSelf == false){
            DisablePlayerInput();
            controlPanel.SetActive(true);
        }else{
            EnablePlayerInput();
            controlPanel.SetActive(false);
        }
    }
    void DisablePlayerInput(){
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            currentPlayers[i].GetComponent<PlayerInput>().enabled = false;
        }
    }
    void EnablePlayerInput(){
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            currentPlayers[i].GetComponent<PlayerInput>().enabled = true;
            PlayerInput input = currentPlayers[i].GetComponent<PlayerInput>();
            InputUser.PerformPairingWithDevice(Keyboard.current, input.user, InputUserPairingOptions.None);
        }
    }
    void UpdateUI(int i){ //fetch player data and update interface
        float percent = currentPlayers[i].percentage;
        string displayPercent = String.Format("{0:F1}%", percent);
        currentPercentages[i].text = (displayPercent);
    }
    void CalculateBounds(){
        Vector3 focusPosition = focus.transform.position;
        Vector3 upperCamLimit = new Vector3(focusPosition.x + id.boundsX, focusPosition.y + id.boundsY, focusPosition.z + id.boundsZ);
        Vector3 bottomCamLimit = new Vector3(focusPosition.x - id.boundsX, focusPosition.y - id.boundsY, focusPosition.z - id.boundsZ);
        Vector3 upperBlastLimit = new Vector3(upperCamLimit.x + blastLineRange, upperCamLimit.y + blastLineRange, upperCamLimit.z);
        Vector3 bottomBlastLimit = new Vector3(bottomCamLimit.x - blastLineRange, bottomCamLimit.y - blastLineRange, bottomCamLimit.z);
        stagebounds = new Bounds(focusPosition, Vector3.zero);
        blastbounds = new Bounds(focusPosition, Vector3.zero);
        //draw stage bounds from upper and lower limit from battle stage id
        stagebounds.Encapsulate(upperCamLimit);
        stagebounds.Encapsulate(bottomCamLimit);

        blastbounds.Encapsulate(upperBlastLimit);
        blastbounds.Encapsulate(bottomBlastLimit);

        playerbounds = new Bounds(focusPosition, Vector3.zero);
        cambounds = new Bounds(focusPosition, Vector3.zero);
        for (int i = 0; i < currentPlayers.Count; i++)
        {     
            //autodetect inner players to act as bounds center
            Vector3 point = currentPlayers[i].transform.position;
            if(stagebounds.Contains(point)){
                playerbounds = new Bounds(point, Vector3.zero);
                cambounds = new Bounds(point, Vector3.zero);
                break;
            }
        }
        if(cambounds.center == focusPosition){ //if no players, are inside, force calculate current bounds as center
            Vector3 clampedPoint = currentPlayers[0].transform.position;
            clampedPoint.x = Mathf.Clamp(clampedPoint.x, stagebounds.min.x, stagebounds.max.x);
            clampedPoint.y = Mathf.Clamp(clampedPoint.y, stagebounds.min.y, stagebounds.max.y);
            clampedPoint.z = Mathf.Clamp(clampedPoint.z, stagebounds.min.z, stagebounds.max.z);
            Bounds extremebound = new Bounds(clampedPoint, Vector3.zero);
            for (int i = 0; i < currentPlayers.Count; i++)
            {
                Vector3 point = currentPlayers[i].transform.position;
                point.x = Mathf.Clamp(point.x, stagebounds.min.x, stagebounds.max.x);
                point.y = Mathf.Clamp(point.y, stagebounds.min.y, stagebounds.max.y);
                point.z = Mathf.Clamp(point.z, stagebounds.min.z, stagebounds.max.z);
                cambounds.Encapsulate(point);
            }
            cambounds = new Bounds(extremebound.center, Vector3.zero);
            playerbounds = new Bounds(extremebound.center, Vector3.zero);
        }
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            //draw player bounds and camera bounds
            Vector3 point = currentPlayers[i].transform.position;
            playerbounds.Encapsulate(point); //before clamping
            
            point.x = Mathf.Clamp(point.x, stagebounds.min.x, stagebounds.max.x);
            point.y = Mathf.Clamp(point.y, stagebounds.min.y, stagebounds.max.y);
            point.z = Mathf.Clamp(point.z, stagebounds.min.z, stagebounds.max.z);
            cambounds.Encapsulate(point); //after clamping
        }
        camCenter = cambounds.center;
        camTargetDistance.x = cambounds.size.x;
        camTargetDistance.y = cambounds.size.y;
        playerDistance.x = playerbounds.size.x;
        playerDistance.y = playerbounds.size.y;
    }
    void CheckBlast(int i){
        Vector3 point = currentPlayers[i].transform.position;
        if(!blastbounds.Contains(point)){
            if(currentPlayers[i].isAlive){
                Debug.Log(currentPlayers[i].name + " is dead.");
                currentPlayers[i].isAlive = false;
                currentPlayers[i].Knockout();
                currentIndicators[i].gameObject.SetActive(false);
                StartCoroutine(RespawnPlayer(i));
            }
        }
    }

    IEnumerator RespawnPlayer(int i){
        yield return new WaitForSeconds(1f);
        currentPlayers[i].transform.position = new Vector3(stagebounds.center.x, stagebounds.max.y, 0f);
        currentPlayers[i].Respawn();
    }

    void PrepareGame(){
        //set battle stage and its properties
        Instantiate(id.asset, Vector3.zero, Quaternion.identity);
        music.clip = id.music;
        music.Play();
        //instantiate active players and set properties
        for (int i = 0; i < active.Length; i++)
        {
            if(active[i]){
                Vector3 playerPosition = id.playerPosition[i];
                string name = "Player" + (i+1);

                GameObject player = Instantiate(playerPrefab, playerPosition, Quaternion.identity);
                player.name = name;
                PlayerInput input = player.GetComponent<PlayerInput>();
                input.defaultControlScheme = name;
                InputUser.PerformPairingWithDevice(Keyboard.current, input.user, InputUserPairingOptions.None);

                player.transform.position = playerPosition;
                player.transform.rotation = Quaternion.identity;
                PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
                playerMovement.playerNumber = i+1;
                playerMovement.groundMask = groundLayer;

                int selfLayer = LayerMask.GetMask((name), (name + "jumpthrough"));
                int opponentLayer = playerLayer.value - selfLayer;

                playerMovement.opponentLayers = opponentLayer;
                playerMovement.globalFunctions = GetComponent<GlobalFunctions>();
                playerMovement.particle = particlePool;
                currentPlayers.Add(playerMovement);
            }
        }
        //set character ID for current players
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            int n = currentPlayers[i].playerNumber - 1;
            if(active[n] && player_ID[n] != null){
                currentPlayers[i].characterID = player_ID[n];
                currentPlayers[i].PlayerSetup();
            }
        }
    }
    void OnEnable(){
        inputAction.Enable();
    }
    void OnDisable(){
        inputAction.Disable();
    }
    void OnDrawGizmos(){
        //stagebounds = green box
        //cambounds = yellow box
        //playerbounds = within yellow box, red box past stage bounds
        if(debug_drawBounds){
            Vector3 focusPosition = focus.transform.position;
            Vector3 upperCamLimit = new Vector3(focusPosition.x + id.boundsX, focusPosition.y + id.boundsY, focusPosition.z + id.boundsZ);
            Vector3 bottomCamLimit = new Vector3(focusPosition.x - id.boundsX, focusPosition.y - id.boundsY, focusPosition.z - id.boundsZ);
            Vector3 upperBlastLimit = new Vector3(upperCamLimit.x + blastLineRange, upperCamLimit.y + blastLineRange, upperCamLimit.z);
            Vector3 bottomBlastLimit = new Vector3(bottomCamLimit.x - blastLineRange, bottomCamLimit.y - blastLineRange, bottomCamLimit.z);
            Bounds stagebounds = new Bounds(focusPosition, Vector3.zero);
            Bounds blastbounds = new Bounds(focusPosition, Vector3.zero);
            //draw stage bounds from upper and lower limit from battle stage id
            stagebounds.Encapsulate(upperCamLimit);
            stagebounds.Encapsulate(bottomCamLimit);

            blastbounds.Encapsulate(upperBlastLimit);
            blastbounds.Encapsulate(bottomBlastLimit);

            Gizmos.color = new Color(0, 255, 0);
            Bounds playerbounds = new Bounds(focusPosition, Vector3.zero);
            Bounds cambounds = new Bounds(focusPosition, Vector3.zero);
            for (int i = 0; i < currentPlayers.Count; i++)
            {
                if(currentPlayers[i] != null){     
                    //autodetect inner players to act as bounds center
                    Vector3 point = currentPlayers[i].transform.position;
                    if(stagebounds.Contains(point)){
                        playerbounds = new Bounds(point, Vector3.zero);
                        cambounds = new Bounds(point, Vector3.zero);
                        break;
                    }
                }
            }
            if(cambounds.center == focusPosition){ //if no players, are inside, force calculate current bounds as center
                Vector3 clampedPoint = currentPlayers[0].transform.position;
                clampedPoint.x = Mathf.Clamp(clampedPoint.x, stagebounds.min.x, stagebounds.max.x);
                clampedPoint.y = Mathf.Clamp(clampedPoint.y, stagebounds.min.y, stagebounds.max.y);
                clampedPoint.z = Mathf.Clamp(clampedPoint.z, stagebounds.min.z, stagebounds.max.z);
                Bounds extremebound = new Bounds(clampedPoint, Vector3.zero);
                for (int i = 0; i < currentPlayers.Count; i++)
                {
                    if(currentPlayers[i] != null){
                        Vector3 point = currentPlayers[i].transform.position;
                        point.x = Mathf.Clamp(point.x, stagebounds.min.x, stagebounds.max.x);
                        point.y = Mathf.Clamp(point.y, stagebounds.min.y, stagebounds.max.y);
                        point.z = Mathf.Clamp(point.z, stagebounds.min.z, stagebounds.max.z);
                        cambounds.Encapsulate(point);
                    }
                }
                cambounds = new Bounds(extremebound.center, Vector3.zero);
                playerbounds = new Bounds(extremebound.center, Vector3.zero);
            }
            for (int i = 0; i < currentPlayers.Count; i++)
            {
                if(currentPlayers[i] != null){
                    //draw player bounds and camera bounds
                    Vector3 point = currentPlayers[i].transform.position;
                    playerbounds.Encapsulate(point); //before clamping
                    
                    point.x = Mathf.Clamp(point.x, stagebounds.min.x, stagebounds.max.x);
                    point.y = Mathf.Clamp(point.y, stagebounds.min.y, stagebounds.max.y);
                    point.z = Mathf.Clamp(point.z, stagebounds.min.z, stagebounds.max.z);
                    cambounds.Encapsulate(point); //after clamping
                    Gizmos.DrawSphere(point, 1);
                }
            }
            Gizmos.color = new Color(0, 0, 255);
            Gizmos.DrawWireCube(stagebounds.center, stagebounds.size);
            Gizmos.color = new Color(255, 255, 0);
            Gizmos.DrawWireCube(blastbounds.center, blastbounds.size);
            Gizmos.color = new Color(255, 0, 0);
            Gizmos.DrawWireCube(playerbounds.center, playerbounds.size);
            Gizmos.color = new Color(0, 255, 0);
            Gizmos.DrawWireCube(cambounds.center, cambounds.size);
            Gizmos.DrawSphere(cambounds.center, 0.5f);
        }
    }
}
