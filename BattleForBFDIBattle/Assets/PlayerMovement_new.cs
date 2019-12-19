using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement_new : MonoBehaviour
{
    CharacterController charControl;
    Vector3 verticalVelocity;
    Vector3 horizontalVelocity;
    public Transform groundCheck;
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

    //Character Attributes
    float walkAcceleration, jumpHeight, gravity, airAcceleration, airFriction, airSpeed, traction;
    int maxJumps;

    void Start()
    {
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

        //Check if gravity is positive by mistake
        if (gravity > 0){
            gravity = (gravity) - (gravity * 2);
        }
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
        float x = Input.GetAxis("horizontal");
        float y = Input.GetAxis("vertical");

        if(Input.GetButtonDown("horizontal")){
            if((Time.time - lastTapMoveTime) < tapMoveSpeed && lastTapMoveValue == Input.GetAxisRaw("horizontal") && isGrounded){
                isDashed = true;
                Debug.Log("Dash");
            }
            lastTapMoveValue = Input.GetAxisRaw("horizontal");
            lastTapMoveTime = Time.time;
        }

        if(x != 0){
            if(isGrounded){
                horizontalVelocity = Vector3.right * x * walkAcceleration;
            }else{
                horizontalVelocity = Vector3.right * x * airAcceleration;
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
                horizontalVelocity.x *= 2f;
            }
        charControl.Move(horizontalVelocity * Time.deltaTime);

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

        charControl.Move(verticalVelocity * Time.deltaTime);
    }
    void Jump(){

        if(isGrounded){// We're grounded, so we can jump.
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsMade += 1;
            Debug.Log("Ground Jump");
        }else{// We're not on the ground, check if we can jump again.
            if(jumpsMade < maxJumps){
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpsMade += 1;
                Debug.Log("Air Jump");
            }
        }
    }
}
