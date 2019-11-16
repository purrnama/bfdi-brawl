using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block_Controller : MonoBehaviour
{
    public bool isBlocking = false;
    SpriteRenderer sprite;
    public Animator anim;
    public Transform playerRef;
    public float currentCooldown, maxCooldown;
    Vector3 mainScale;

    void Start(){
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        mainScale = transform.localScale;
    }
    void Update()
    {
        isBlocking = playerRef.GetComponent<Player_Controller>().blocking;
        float clamp = Mathf.Clamp(currentCooldown, 0f, maxCooldown);
        transform.localScale = mainScale * (clamp/maxCooldown);
        if(isBlocking){
            sprite.enabled = true;
            anim.enabled = true;
            currentCooldown -= Time.deltaTime;
            if(currentCooldown < 0){
                playerRef.GetComponent<Player_Controller>().UnBlock();
                playerRef.gameObject.GetComponent<Player_Controller>().BlockStun();
            }
        }else{
            if(currentCooldown < maxCooldown){
            currentCooldown += Time.deltaTime;
            }
        }
    }
    public void Damaged(){
        anim.SetTrigger("Damaged");
    }

    public void endBlock(){
        anim.enabled = false;
        sprite.enabled = false;
    }
}
