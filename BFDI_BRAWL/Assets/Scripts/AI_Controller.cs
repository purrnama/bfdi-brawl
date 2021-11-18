using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Controller : MonoBehaviour
{
    PlayerMovement player;
    Attack_Controller attack;
    Game_Manager manager;

    [SerializeField] PlayerMovement targetPlayer;

    internal List<PlayerMovement> currentPlayers = new List<PlayerMovement>();

    void Awake()
    {
        player = GetComponent<PlayerMovement>();
        attack = GetComponent<Attack_Controller>();
    }
    
    void Update()
    {
        player.AIMove(Approach(targetPlayer, 4f));
    }
    Vector2 Approach(PlayerMovement target, float range){ //approach a player until reaching a certain range
        Vector2 output = Vector2.zero;
        if(target != null){
            Vector3 targetPos = target.transform.position;
            float distance = transform.position.x - targetPos.x;
            distance = Mathf.Abs(distance);
            if(distance > range){ // move when target exceeds range
                if(targetPos.x > transform.position.x){ // target is to the right
                    output = new Vector2(1, 0); // go right
                }else{
                    output = new Vector2(-1, 0); //go left
                }
            }else{
                output = Vector2.zero; // stop when within range
            }
            
        }
        return output;
    }
    void FindNearestPlayer(){
        //From current players, check if alive and compare 
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            if(currentPlayers[i].isAlive){
                float distance = Vector3.Distance(gameObject.transform.position, currentPlayers[i].transform.position);
                if(distance != 0f){

                }
            }
        }
    }

    Vector2 randommovement(){ //gives a random x input. looks like a seizure.
        Vector2 output = Vector2.zero;
        float random = Random.Range(-1,2);
        Debug.Log(random);
        if(random > 0){
            output = new Vector2(1,0);
        }else if(random < 0){
            output = new Vector2(-1,0);
        }else{
            output = new Vector2(0,0);
        }
    return output;
    }
}
