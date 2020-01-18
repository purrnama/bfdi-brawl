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
    float lastTapMoveTime = 0f;
    float lastTapMoveValue = 0f;
    float lastTapAttackTime = 0f;
    float lastTapAttackValue = 0f;
    float jumpRate;
    float attackRate;
    float nextJumpTime = 0f;
    float nextAttackTime = 0f;
    public LayerMask opponentLayers;
    public Transform attackPoint;
    public bool active;
    bool hitstun = false, hitlag = false;
    public bool attackLagComplete = false;


    //Character Attributes
    float walkAcceleration, jumpHeight, airAcceleration, airFriction, airSpeed, traction, dashAcceleration, attackRange;
    float gravity, weight, knockbackScaling, percentage;
    float jabDamage, nAirDamage, fSmashDamage, fAirDamage, uSmashDamage, uAirDamage, dSmashDamage, dAirDamage, fDashDamage;
    float jabAngle, nAirAngle, fSmashAngle, fAirAngle, uSmashAngle, uAirAngle, dSmashAngle, dAirAngle, fDashAngle;
    Vector2 attackDistance;
    int maxJumps;
    bool isJumpingSnapped;

    void Start()
    {
        //Check if characterID is available
        if(characterID == null){
            Debug.LogError("No characterID found!");
            return;
        }else{

            //Fetch attributes from CharacterID
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
            attackRate = characterID.attackRate;
            attackRange = characterID.attackRange;
            attackDistance = characterID.attackDistance;
            isJumpingSnapped = characterID.isJumpingSnapped;
            dashAcceleration = characterID.dashAcceleration;
            knockbackScaling = characterID.knockbackScaling;
            weight = characterID.weight;
            groundCheck = transform.GetChild(0);
            attackPoint = transform.GetChild(1);

            jabDamage = characterID.jabDamage;
            nAirDamage = characterID.nAirDamage;
            fSmashDamage = characterID.fSmashDamage;
            fAirDamage = characterID.fAirDamage;
            uSmashDamage = characterID.uSmashDamage;
            uAirDamage = characterID.uAirDamage;
            dSmashDamage = characterID.dSmashDamage;
            dAirDamage = characterID.dAirDamage;
            fDashDamage = characterID.fDashDamage;

            jabAngle = characterID.jabAngle;
            nAirAngle = characterID.nAirAngle;
            fSmashAngle = characterID.fSmashAngle;
            fAirAngle = characterID.fAirAngle;
            uSmashAngle = characterID.uSmashAngle;
            uAirAngle = characterID.uAirAngle;
            dSmashAngle = characterID.dSmashAngle;
            dAirAngle = characterID.dAirAngle;
            fDashAngle = characterID.fDashAngle;

            //Check if gravity is positive by mistake
            if (gravity > 0){
                gravity = (gravity) - (gravity * 2);
                Debug.LogWarning("Gravity value in " + characterID.characterName + "'s CharacterID is positive. It is flipped automatically, but consider making it negative");
            }
        }
        //Check if attackPoint is added
        if(attackPoint == null){
            Debug.LogError("attackPoint not assigned!");
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
            jumpsMade = 0; // Reset jumps when on ground
        }

        //Store movement inputs
        x = Input.GetAxis("horizontal");
        y = Input.GetAxis("vertical");


        if(x != 0 && active && !hitstun){
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
        if(Input.GetButtonDown("horizontal") && active && !hitstun){
                if((Time.time - lastTapMoveTime) < tapMoveSpeed && lastTapMoveValue == Input.GetAxisRaw("horizontal") && isGrounded){
                    isDashed = true;
                    Debug.Log("Dash");
                }
                lastTapMoveValue = Input.GetAxisRaw("horizontal");
                lastTapMoveTime = Time.time;
        }
        //Jump mechanism
        if(Input.GetButtonDown("jump") && active && !hitstun){
                if(Time.time >= nextJumpTime){ // Prevent spamming
                    Jump();
                    nextJumpTime = Time.time +(1f / jumpRate);
                }
        }
        //Attack mechanicsm
        if(Input.GetButtonDown("attack") && active && !hitlag){
            if(Time.time >= nextAttackTime){
                LaggingAttack();
                nextAttackTime = Time.time + (1f / attackRate);
            }
        }
        //Gravity
        if(verticalVelocity.y > -airSpeed){
            verticalVelocity.y += gravity * Time.deltaTime;
        }
        //Store current attack point position
        Vector3 attackPosition = attackPoint.localPosition;

        //Change attack point position
        if(horizontalVelocity.x > 0){
            attackPosition.x = attackDistance.x;
        }else if(horizontalVelocity.x < 0){
            attackPosition.x = -attackDistance.x;
        }
        //Override position for vertical attacks
        if(y > 0){
            attackPosition.y = attackDistance.y;
            attackPosition.x = 0f;
        }else if(y < 0){
            attackPosition.y = -attackDistance.y;
            attackPosition.x = 0f;
        }else{
            attackPosition.y = 0f;
        }
        //Update attack point position
        attackPoint.localPosition = attackPosition;

        //Move the player when there is no hitlag
        if(!hitlag){
            charControl.Move(horizontalVelocity * Time.deltaTime);
            charControl.Move(verticalVelocity * Time.deltaTime);
        }
    }
    void Jump(){
        isDashed = false; //Disable dashing
        if(isGrounded){ // We're grounded, so we can jump.
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Jump velocity formula
            jumpsMade += 1;
            //Debug.Log("Ground Jump");
                if(isJumpingSnapped){ //Jumping velocity flips when air jumping to opposite direction
                    if(horizontalVelocity.x < 0 && Input.GetAxisRaw("horizontal") > 0){
                        horizontalVelocity.x = Mathf.Abs(horizontalVelocity.x);
                    }
                    if(horizontalVelocity.x > 0 && Input.GetAxisRaw("horizontal") < 0){
                        horizontalVelocity.x = horizontalVelocity.x - (horizontalVelocity.x * 2);
                    }
                }else{
                    horizontalVelocity.x /= 2f;
                }
        }else{// We're not on the ground, check if we can jump again.
            if(jumpsMade < maxJumps){
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpsMade += 1;
                //Debug.Log("Air Jump");
                if(isJumpingSnapped){
                    if(horizontalVelocity.x < 0 && Input.GetAxisRaw("horizontal") > 0){
                        horizontalVelocity.x = (Mathf.Abs(horizontalVelocity.x)) / 2f;
                    }
                    if(horizontalVelocity.x > 0 && Input.GetAxisRaw("horizontal") < 0){
                        horizontalVelocity.x = (horizontalVelocity.x - (horizontalVelocity.x * 2f)) / 2f;
                    }
                }else{
                    horizontalVelocity.x /= 2f;
                }
            }
        }
    }

    //Returns a knockback value if an attack is delivered. Based on SSB formula.
    float KnockbackValue(float percentage, float damage, float weight, float scaling, float baseknockback){
        return ((((((percentage / 10f) + ((percentage * damage) / 20f)) * weight * 1.4f) + 18f) * scaling) + baseknockback);
    }

    //Coroutine for hitlag
    IEnumerator Hitlag(float d){
        hitlag = true;
        yield return new WaitForSeconds((d * 0.65f + 6f) * Time.deltaTime);
        hitlag = false;
    }

    //Couroutine for targetted player hitlag. Delivers knockback after hitlag.
    IEnumerator HitlagTarget(float d, float h, float v, float o){
        hitlag = true;
        yield return new WaitForSeconds((d * 0.65f + 6f) * Time.deltaTime);
        horizontalVelocity = Vector3.right * h * o;
        verticalVelocity = Vector3.up * v;
        hitlag = false;
    }

    //Starts a couroutine (the actual attack) and wait for the attack lag to complete. The lag is created by animation.
    void LaggingAttack(){
        attackLagComplete = false;
        StartCoroutine(Attack());
    }

    public void CompleteAttackLag(){
        attackLagComplete = true;
    }

    //Coroutine for attack. Delivers attack after
    IEnumerator Attack(){

        //Store variables
        float damageValue = 0f;
        int horizState = 0; //-1 = left, 1 = right
        int vertState = 0; //-1 = down, 1 = up
        float recordedX = x;
        float recordedY = y;
        bool recordedDash = isDashed;

        //Detect type of attack, store them in variables
        Vector3 attackPosition = attackPoint.localPosition;
        if(attackPosition.x > 0){ //AttackPoint is on the right
            horizState = 1;
            vertState = 0;
            if(x > 0){ //Input is also to the right
                if(isGrounded){
                    if(isDashed){
                        damageValue = fDashDamage;
                        Debug.Log("Forward Dash Attack R");
                    }else{
                        damageValue = fSmashDamage;
                        Debug.Log("Forward Ground Attack R");
                    }
                }
                else{
                    damageValue = fAirDamage;
                    Debug.Log("Forward Aerial Attack R");
                }
            }
        }else{
            horizState = -1;
            vertState = 0;
            if(x < 0){
                if(isGrounded){
                    if(isDashed){
                        damageValue = fDashDamage;
                        Debug.Log("Forward Dash Attack L");
                    }else{
                        damageValue = fSmashDamage;
                        Debug.Log("Forward Ground Attack L");
                    }
                }else{
                    damageValue = fAirDamage;
                    Debug.Log("Forward Aerial Attack L");
                }
            }
        }
        //Override attack variables for vertical attacks
        if(attackPosition.y > 0){
            vertState = 1;
            if(isGrounded){
                damageValue = uSmashDamage;
                Debug.Log("Up Ground Attack");
            }else{
                damageValue = uAirDamage;
                Debug.Log("Up Aerial Attack");
            }
        }else if(attackPosition.y < 0){
            vertState = 1;
            if(isGrounded){
                damageValue = dSmashDamage;
                Debug.Log("Down Ground Attack");
            }else{
                damageValue = dAirDamage;
                Debug.Log("Down Aerial Attack");
            }
        }else{
            vertState = 0;
        }
        if(x == 0 && y == 0 && isGrounded){
            damageValue = jabDamage;
            Debug.Log("Neutral Ground Attack");
        }else if(x == 0 && y == 0 && !isGrounded){
            damageValue = nAirDamage;
            Debug.Log("Neutral Aerial Attack");
        }

        Debug.Log("Waiting for attack lag to complete.");
        yield return new WaitUntil(() => attackLagComplete == true);

        //Create a sphere to detect opponents within the attack range of the player.
        Collider[] hitOpponents = Physics.OverlapSphere(attackPoint.position, attackRange, opponentLayers);

        //If hit opponent, apply hitlag
        if(hitOpponents.Length > 0){
            StartCoroutine(Hitlag(damageValue));
        }
        //For every opponent within area of attack range, damage them.
        foreach(Collider opponent in hitOpponents){
            PlayerMovement_new opp = opponent.GetComponent<PlayerMovement_new>();
            opp.percentage += damageValue; //Add percentage first
            float knockback = KnockbackValue(opp.percentage, damageValue, opp.weight, opp.knockbackScaling/100, damageValue);
            float angle = 0f;

            if(recordedX == 0 && vertState == 0 && isGrounded){
                angle = jabAngle;
            }else if(recordedX == 0 && vertState == 0 && !isGrounded){
                angle = nAirAngle;
            }else if(horizState != 0 && vertState == 0 && isGrounded){
                if(recordedDash){
                    angle = fDashAngle;
                }else{
                    angle = fSmashAngle;
                }
            }else if(horizState != 0 && vertState == 0 && !isGrounded){
                angle = fAirAngle;
            }else if(vertState == 1 && isGrounded){
                angle = uSmashAngle;
            }else if(vertState == 1 && !isGrounded){
                angle = uAirAngle;
            }else if(vertState == -1 && isGrounded){
                angle = dSmashAngle;
            }else if(vertState == -1 && !isGrounded){
                angle = dAirAngle;
            }
            opp.Damage(angle, knockback, horizState, damageValue);
            Debug.Log("Hit " + opponent.name + ". Horizontal = " + horizState + " Vertical = " + vertState + " Angle = " + angle);
        }
    }
    IEnumerator Hitstun(float h){
        hitstun = true;
        yield return new WaitForSeconds(h * Time.deltaTime);
        hitstun = false;
    }

    //Calculate perpendicular vectors according to knockback and angle
    public void Damage(float angle, float knockback, int orient, float damage){
        float horizV = knockback * Mathf.Sin(angle);
        float vertV = knockback * Mathf.Cos(angle);

        float hitstunValue = 32f + (0.1455f * (knockback - 82.5f));


        StartCoroutine(Hitstun(hitstunValue));
        StartCoroutine(HitlagTarget(damage, horizV, vertV, orient));
        //Debug.Log(angle + ", " + knockback + ", " + orient + ", " + (hitstunValue * Time.deltaTime));
    }

    //Draw gizmos to ease editor production
    void OnDrawGizmosSelected() {
        if(attackPoint == null || characterID == null){
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, characterID.attackRange);
    }
}
