using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement_new : MonoBehaviour
{
    CharacterController charControl;
    Vector3 verticalVelocity;
    Vector3 horizontalVelocity;
    Transform groundCheck;
    float x, y;
    bool isGrounded;
    bool isDashed;
    int jumpsMade = 0;
    public float groundDistance;
    public LayerMask groundMask;
    public Character_ID characterID;
    public float tapMoveSpeed;
    float lastTapMoveTime = 0;
    float lastTapMoveValue = 0;
    float jumpRate;
    float nextJumpTime = 0f;
    public LayerMask opponentLayers;
    float attackRange = 0.5f;
    Transform attackPoint;

    //Character Attributes
    float walkAcceleration, jumpHeight, gravity, airAcceleration, airFriction, airSpeed, traction, dashAcceleration;
    int maxJumps;
    bool isJumpingSnapped;

    void Start()
    {
        if(characterID == null){
            Debug.LogError("No characterID found.");
            return;
        }else{

            //Get attributes from CharacterID
            charControl = gameObject.GetComponent<CharacterController>();
            walkAcceleration = characterID.walkAcceleration;
            jumpHeight = characterID.jumpHeight;
            gravity = characterID.gravity;
            airAcceleration = characterID.airAcceleration;
            airSpeed = characterID.airSpeed;
            airFriction = characterID.airFriction;
            traction = characterID.traction;
            maxJumps = characterID.maxJumps;
            jumpRate = characterID.jumpRate;
            isJumpingSnapped = characterID.isJumpingSnapped;
            dashAcceleration = characterID.dashAcceleration;
            groundCheck = transform.GetChild(0);
            attackPoint = transform.GetChild(1);

            //Check if gravity is positive by mistake
            if (gravity > 0){
                gravity = (gravity) - (gravity * 2);
                Debug.LogWarning("Gravity value in " + characterID.characterName + "'s CharacterID is positive. It is flipped automatically, but consider making it negative");
            }
        }
    }

    public void Reset(){
        if(characterID == null){
        }else{

            //Get attributes from CharacterID
            charControl = gameObject.GetComponent<CharacterController>();
            walkAcceleration = characterID.walkAcceleration;
            jumpHeight = characterID.jumpHeight;
            gravity = characterID.gravity;
            airAcceleration = characterID.airAcceleration;
            airSpeed = characterID.airSpeed;
            airFriction = characterID.airFriction;
            traction = characterID.traction;
            maxJumps = characterID.maxJumps;
            jumpRate = characterID.jumpRate;
            isJumpingSnapped = characterID.isJumpingSnapped;
            dashAcceleration = characterID.dashAcceleration;
            groundCheck = transform.GetChild(0);
            attackPoint = transform.GetChild(1);

            //Check if gravity is positive by mistake
            if (gravity > 0){
                gravity = (gravity) - (gravity * 2);
                Debug.LogWarning("Gravity value in " + characterID.characterName + "'s CharacterID is positive. It is flipped automatically, but consider making it negative");
            }
        }
        horizontalVelocity = Vector3.zero;
        verticalVelocity = Vector3.zero;
        transform.position = new Vector3(0f, 7.6f, 0f);
        Debug.Log("Player reset");
    }

    void Update()
    {
        //Checks for ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && verticalVelocity.y < 0){
            verticalVelocity.y = -2f;
            jumpsMade = 0; // Reset jumps
        }

        //Store movement inputs
        x = Input.GetAxis("horizontal");
        y = Input.GetAxis("vertical");


        if(x != 0){
            if(isGrounded){
                horizontalVelocity = Vector3.right * x * walkAcceleration;
            }else{ // Inherits velocity from ground, limited by air speed
                if(horizontalVelocity.x < airSpeed && horizontalVelocity.x > -airSpeed){
                    horizontalVelocity += Vector3.right * x * airAcceleration * Time.deltaTime;
                }
            }
        }else{
            if(isGrounded){
                horizontalVelocity.x /= 1 + traction * Time.deltaTime;
            }else{
                horizontalVelocity.x /= 1 + airFriction * Time.deltaTime;
            }
            isDashed = false;
        }
            if(isDashed && isGrounded){
                horizontalVelocity = Vector3.right * x * dashAcceleration;
            }
        //Dash mechanism
        if(Input.GetButtonDown("horizontal")){
            if((Time.time - lastTapMoveTime) < tapMoveSpeed && lastTapMoveValue == Input.GetAxisRaw("horizontal") && isGrounded){
                isDashed = true;
                Debug.Log("Dash");
            }
            lastTapMoveValue = Input.GetAxisRaw("horizontal");
            lastTapMoveTime = Time.time;
        }
        //Jump mechanism
        if(Input.GetButtonDown("jump")){
            
            if(Time.time >= nextJumpTime){ // Prevent spamming
                Jump();
                nextJumpTime = Time.time +(1f / jumpRate);
            }
        }

        if(verticalVelocity.y > -airSpeed){
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        charControl.Move(horizontalVelocity * Time.deltaTime);
        charControl.Move(verticalVelocity * Time.deltaTime);
    }
    void Jump(){
        isDashed = false;
        if(isGrounded){// We're grounded, so we can jump.
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsMade += 1;
            Debug.Log("Ground Jump");
                if(isJumpingSnapped){
                    if(horizontalVelocity.x < 0 && Input.GetAxisRaw("horizontal") > 0){
                        horizontalVelocity.x = Mathf.Abs(horizontalVelocity.x);
                        Debug.Log("Jump snapped");
                    }
                    if(horizontalVelocity.x > 0 && Input.GetAxisRaw("horizontal") < 0){
                        horizontalVelocity.x = horizontalVelocity.x - (horizontalVelocity.x * 2);
                        Debug.Log("Jump snapped");
                    }
                }else{
                    horizontalVelocity.x /= 2f;
                }
        }else{// We're not on the ground, check if we can jump again.
            if(jumpsMade < maxJumps){
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpsMade += 1;
                Debug.Log("Air Jump");
                if(isJumpingSnapped){
                    if(horizontalVelocity.x < 0 && Input.GetAxisRaw("horizontal") > 0){
                        horizontalVelocity.x = (Mathf.Abs(horizontalVelocity.x)) / 2f;
                        Debug.Log("Jump snapped");
                    }
                    if(horizontalVelocity.x > 0 && Input.GetAxisRaw("horizontal") < 0){
                        horizontalVelocity.x = (horizontalVelocity.x - (horizontalVelocity.x * 2f)) / 2f;
                        Debug.Log("Jump snapped");
                    }
                }else{
                    horizontalVelocity.x /= 2f;
                }
            }
    }

    void Attack(){

        Collider[] hitOpponents = Physics.OverlapSphere(attackPoint.position, attackRange, opponentLayers);

        foreach(Collider opponent in hitOpponents){
            Debug.Log("Hit " + opponent.name);
        }

        }
    }
}
