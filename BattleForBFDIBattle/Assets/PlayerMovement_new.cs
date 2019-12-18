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
    public float groundDistance;
    public LayerMask groundMask;
    public Character_ID characterID;
    public float tapSpeed;
    float lastTapTime = 0;
    float lastTapValue = 0;

    //Character Attributes
    float walkAcceleration, jumpHeight, gravity, airAcceleration, airFriction, airSpeed, traction;

    void Start()
    {
        charControl = gameObject.GetComponent<CharacterController>();
        walkAcceleration = characterID.walkAcceleration;
        jumpHeight = characterID.jumpHeight;
        gravity = characterID.gravity;
        airAcceleration = characterID.airAcceleration;
        airSpeed = characterID.airSpeed;
        airFriction = characterID.airFriction;
        traction = characterID.traction;

        if (gravity > 0){ //Check if gravity is positive by mistake
            gravity = (gravity) - (gravity * 2);
        }
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && verticalVelocity.y < 0){
            verticalVelocity.y = -2f;
        }

        float x = Input.GetAxis("horizontal");
        float y = Input.GetAxis("vertical");

        if(Input.GetButtonDown("horizontal")){
            Debug.Log("pressed");
            if((Time.time - lastTapTime) < tapSpeed && lastTapValue == Input.GetAxisRaw("horizontal") && isGrounded){
                isDashed = true;
                Debug.Log("double pressed");
            }
            lastTapValue = Input.GetAxisRaw("horizontal");
            lastTapTime = Time.time;
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

        if(Input.GetButtonDown("jump") && isGrounded){
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        if(verticalVelocity.y > -airSpeed){
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        charControl.Move(verticalVelocity * Time.deltaTime);
    }
}
