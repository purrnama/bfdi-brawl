using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] internal int playerNumber = 0;
    [SerializeField] internal GlobalFunctions globalFunctions = null;
    internal AudioSource sound;
    internal Vector2 movementInput;
    CharacterController charControl;
    internal Animator anim = null;
    internal SpriteRenderer sprite = null;
    internal Attack_Controller attack = null;
    float charRadius;
    float charHeight;
    Vector3 verticalVelocity;
    Vector3 horizontalVelocity;
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private ShieldController shield = null;
    internal float x, y;
    internal bool isGrounded;
    internal bool isDashed;
    internal bool isIntangible;
    bool isInvincible;
    internal bool isAttacking = false;
    int jumpsMade = 0;
    float airTime = 0f;
    [SerializeField] private float groundDistance = 0f;
    [SerializeField] internal LayerMask groundMask;
    public Character_ID characterID;
    public float tapMoveSpeed;
    float lastTapMoveTime = 0f;
    float lastTapMoveValue = 0f;
    float lastTapAttackTime = 0f;
    float lastTapAttackValue = 0f;
    float jumpRate;
    internal float attackRate;
    float fastFallingPercentage;
    float nextJumpTime = 0f;
    internal float nextAttackTime = 0f;
    [SerializeField] internal LayerMask opponentLayers;
    [SerializeField] internal Transform attackPoint = null;
    [SerializeField] internal bool active;
    internal bool isAlive = true;
    bool hitstun = false, isEdgeGrab = false, isFastFalling = false, isDropPlatform, hasAirDodge = false;
    internal bool hitlag = false, isShielding = false, hasIntangibleEdge = false, isStunned = false, isDodging = false, isCharging = false;
    int EdgeGrabOrient;
    public bool attackLag = false;
    [SerializeField] internal ParticlePool particle;
    [SerializeField] private ParticleSystem blastTrail;
    bool isLandSpawned = true;
    int spotDodgeFrames;
    float spotDodgeTime;

    //Smash Character Attributes
    float walkSpeed, jumpHeight, airAcceleration, airFriction, airSpeed, traction, dashAcceleration;
    internal float attackRange, percentage, weight;
    internal float gravity;
    float knockbackScaling;
    internal Vector3 uNeutral, uAir, uSmash, uTilt, jab, fNeutral, fAir, fSmash, fDash, dTilt, dNeutral, dAir, dSmash;
    internal Vector2 attackDistance;
    int maxJumps;
    bool isJumpingSnapped;
    internal bool isReadAction = false;
    Coroutine stunCoroutine = null;

    internal Vector3 controllerCenter;
    float controllerRadius, controllerHeight;

    internal event Action OnRespawn;

    void Awake()
    {
    }
    void Start()
    {
    }
    internal void PlayerSetup(){ //called by manager to prepare player to game
        //Check if characterID is available
        if(characterID == null){
            Debug.LogError("No characterID found!");
            return;
        }else{

            //Fetch attributes from CharacterID
            charControl = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();
            sound = GetComponent<AudioSource>();
            attack = GetComponent<Attack_Controller>();
            controllerCenter = characterID.center;
            controllerRadius = characterID.radius;
            controllerHeight = characterID.height;
            charControl.center = controllerCenter;
            charControl.radius = controllerRadius;
            charControl.height = controllerHeight;

            walkSpeed = characterID.walkSpeed;
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
            fastFallingPercentage = (characterID.fastFallingPercentage + 100f) / 100f;
            isJumpingSnapped = characterID.isJumpingSnapped;
            dashAcceleration = characterID.dashAcceleration;
            weight = characterID.weight;
            spotDodgeFrames = characterID.spotDodgeFrames;
            spotDodgeTime = spotDodgeFrames * Time.fixedDeltaTime;

            uNeutral = characterID.uNeutral;
            uAir = characterID.uAir;
            uSmash = characterID.uSmash;
            uTilt = characterID.uTilt;
            jab = characterID.jab;
            fNeutral = characterID.fNeutral;
            fAir = characterID.fAir;
            fSmash = characterID.fSmash;
            fDash = characterID.fDash;
            dTilt = characterID.dTilt;
            dNeutral = characterID.dNeutral;
            dAir = characterID.dAir;
            dSmash = characterID.dSmash;

            charRadius = charControl.radius;
            charHeight = charControl.height;
            shield.transform.localPosition = charControl.center;
            blastTrail.transform.localPosition = charControl.center;
            //Check if gravity is positive by mistake
            if (gravity > 0){
                gravity = (gravity) - (gravity * 2);
            }
        }
        //Check if attackPoint is added
        if(attackPoint == null){
            Debug.LogError("attackPoint not assigned!");
        }
        //loading moveset and necessary assets
        gameObject.AddComponent(Type.GetType(characterID.characterName + "_Moveset"));
        Moveset moveset = GetComponent(Type.GetType(characterID.characterName + "_Moveset")) as Moveset;
        attack.moveset = moveset;
        moveset.attack = attack;
        moveset.player = this;
        moveset.id = characterID;

        Debug.Log("Player " + playerNumber + " setup finished.");
    }

    void Update()
    {
        //Checks for ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);


        if(!isGrounded){
            isLandSpawned = false;
        }
        //Grounded
        if(isGrounded && verticalVelocity.y < 0){
            hasIntangibleEdge = false;
            hasAirDodge = false;
            isDropPlatform = false;
            verticalVelocity.y = -2f;
            jumpsMade = 0; // Reset jumps when on ground
            isFastFalling = false;
            if(!isLandSpawned){
                spawnLandParticle();
                isLandSpawned = true;
            }
        }

        //Store movement inputs
        x = movementInput.x;
        y = movementInput.y;

        //Horizontal input is pressed
        if(x != 0 && active && !hitstun && !isDodging && !isStunned && !attackLag && isAlive){
            //When edge grabbing
            if(isEdgeGrab){
                if(EdgeGrabOrient > 0 && x < 0){
                    jumpsMade = 10;
                    isEdgeGrab = false;
                    verticalVelocity.y = Mathf.Sqrt((charHeight * 1.5f) * -2f * gravity);
                }else if(EdgeGrabOrient > 0 && x > 0){
                    isEdgeGrab = false;
                    verticalVelocity.y = Mathf.Sqrt((charHeight * 1.5f) * -2f * gravity);
                    horizontalVelocity.x = walkSpeed * EdgeGrabOrient;
                }else if(EdgeGrabOrient < 0 && x > 0){
                    jumpsMade = 10;
                    isEdgeGrab = false;
                    verticalVelocity.y = Mathf.Sqrt((charHeight * 1.5f) * -2f * gravity);
                }else if(EdgeGrabOrient < 0 && x < 0){
                    isEdgeGrab = false;
                    verticalVelocity.y = Mathf.Sqrt((charHeight * 1.5f) * -2f * gravity);
                    horizontalVelocity.x = walkSpeed * EdgeGrabOrient;
                }
                DisableIntangibility();
            }else{
                if(isGrounded){
                        horizontalVelocity = Vector3.right * x * walkSpeed;
                }else{ // Inherits velocity from ground, limited by air speed
                    if(horizontalVelocity.x < airSpeed && horizontalVelocity.x > -airSpeed){
                        horizontalVelocity += Vector3.right * x * airAcceleration * Time.fixedDeltaTime;
                    }
                }
            }
        }else if(x == 0 && active && !hitstun && isAlive){
            if(isGrounded){
                horizontalVelocity.x /= 1 + traction * Time.fixedDeltaTime;
            }else{
                horizontalVelocity.x /= 1 + airFriction * Time.fixedDeltaTime;
            }
            isDashed = false;
        }
        if(!active){ //Allow friction even when inactive
            if(isGrounded){
                horizontalVelocity.x /= 1 + traction * Time.fixedDeltaTime;
            }else{
                horizontalVelocity.x /= 1 + airFriction * Time.fixedDeltaTime;
            }
        }
        //Dashing
        if(isDashed && isGrounded){
            horizontalVelocity = Vector3.right * x * dashAcceleration;
        }
        //Do not allow player to move when shielding or charging
        if(isShielding || isCharging){
            horizontalVelocity.x = 0f;
        }
        //manipulate air speed and gravity in different conditions
        float airspeed = airSpeed;
        float grav = gravity;
        if(isFastFalling){
            airspeed *= fastFallingPercentage;
            grav *= fastFallingPercentage;
        }
        //Gravity
        if(verticalVelocity.y > -airspeed && !isEdgeGrab){
            verticalVelocity.y += grav * Time.fixedDeltaTime;
        }
        //Stop blast trail when falling
        if(verticalVelocity.y < 0){
            if(blastTrail.isEmitting){
                blastTrail.Stop();
            }
        }
        /*
        if(!isDropPlatform){
            if(verticalVelocity.y > 0){
                gameObject.layer = LayerMask.NameToLayer("Player" + playerNumber + "jumpthrough");
            }else if(verticalVelocity.y < 0){
               gameObject.layer = LayerMask.NameToLayer("Player" + playerNumber);
            }
        }else{
            gameObject.layer = LayerMask.NameToLayer("Player" + playerNumber + "jumpthrough");
        }
        */
        if(verticalVelocity.y > 0){
            gameObject.layer = LayerMask.NameToLayer("Player" + playerNumber + "jumpthrough");
        }else if(verticalVelocity.y < 0){
            if(!isDropPlatform){
                gameObject.layer = LayerMask.NameToLayer("Player" + playerNumber);
            }
        }
        if(y < 0 && isAlive){
            if(isEdgeGrab){
                isEdgeGrab = false;
                jumpsMade = 0;
                DisableIntangibility();
            }
            if(!isGrounded){
                isFastFalling = true;
            }
            if(!isDropPlatform){
                gameObject.layer = LayerMask.NameToLayer("Player" + playerNumber + "jumpthrough");
                isDropPlatform = true;
            }
        }

        //Move the player when there is no hitlag
        if(!hitlag && !isEdgeGrab && isAlive){
            charControl.Move(horizontalVelocity * Time.deltaTime);
            charControl.Move(verticalVelocity * Time.deltaTime);
        }
        //Flip sprites based on velocity direction or edge grab orientation
        if(!isEdgeGrab){
            if (horizontalVelocity.x > 0){
                sprite.flipX = false;
            }else if(horizontalVelocity.x < 0){
                sprite.flipX = true;
            }
        }else{
            if(EdgeGrabOrient == 1){
                sprite.flipX = true;
            }else if(EdgeGrabOrient == -1){
                sprite.flipX = false;
            }
        }
        //Update animation parameters
        if(anim != null && active){
            if(x > 0){
                anim.SetInteger("X", 1);
            }else if(x < 0){
                anim.SetInteger("X", -1);
            }else{
                anim.SetInteger("X", 0);
            }
            anim.SetFloat("Y", verticalVelocity.y);
        }
            anim.SetBool("IsDashed", isDashed);
            anim.SetBool("IsGrounded", isGrounded);
            anim.SetBool("IsHitStun", hitstun);
            anim.SetBool("IsEdgeGrab", isEdgeGrab);
        
        //Counting airtime in frames for edge grab intangibility
        if(!isGrounded && !isEdgeGrab){
            airTime += 1f;
        }else{
            airTime = 0f;
        }
    }
    public void Move(InputAction.CallbackContext context){
        movementInput = context.ReadValue<Vector2>();
        if(gameObject.activeSelf){ //check for actions only when active to prevent coroutine conflict
            StartCoroutine(ReadAction());
        }
    }
    public void AIMove(Vector2 setAxis){
        movementInput = setAxis;
    }
    public void AIJump(){
        Jump();
    }
    IEnumerator ReadAction(){
        isReadAction = true;
        yield return new WaitForSeconds(0.05f); //window for attack
        isReadAction = false;
        if(movementInput.x != 0 && movementInput.y == 0){
            Dash(movementInput.x);
        }
        if(movementInput.y > 0){
            Jump();
        }else if(movementInput.y < 0){
            Duck();
        }
    }
    void Dash(float i){
        if(active && !hitstun && !isDodging && !isShielding && !isStunned && !attackLag && isAlive){
            if((Time.time - lastTapMoveTime) < tapMoveSpeed && lastTapMoveValue == i && isGrounded && !isDashed){
                    isDashed = true;
                    spawnDashParticle();
            }
            lastTapMoveValue = i;
            lastTapMoveTime = Time.time;
        }
    }
    public void Shield(InputAction.CallbackContext context){
        if(context.performed){
            if(active && !hitlag && !isDodging && !isStunned && !attackLag && isAlive){
                if(isGrounded){
                    EnableShield();
                }else{
                    if(!hasAirDodge){
                        AirDodge();
                    }
                }
            }
        }
        if(context.canceled){
            if(active && !hitlag && !attackLag && isAlive){
                if(isGrounded){
                DisableShield();
            }
            }
        }
    }
    public void CancelCharge(){
        isAttacking = false;
        isCharging = false;
    }
    void Duck(){
        if(active && !hitlag && !isDodging && !isStunned && !attackLag && isAlive){
            if(isGrounded){
                if(isShielding){
                    SpotDodge();
                    DisableShield();
                }
            }
        }
    }
    void Jump(){
        if(active && !hitstun && !isDodging && !isShielding && !isStunned && !attackLag && isAlive && !isAttacking && !isCharging){
            if(Time.time >= nextJumpTime){ // Prevent spamming
                isDashed = false; //Disable dashing
                isFastFalling = false;
                blastTrail.Stop();
                if(isEdgeGrab){
                        isEdgeGrab = false;
                        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                        jumpsMade += 1;
                        DisableIntangibility();
                }else{
                    if(isGrounded){ // We're grounded, so we can jump.
                        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Jump velocity formula
                        jumpsMade += 1;
                        //Debug.Log("Ground Jump");
                            if(isJumpingSnapped){ //Jumping velocity flips when air jumping to opposite direction
                                if(horizontalVelocity.x < 0 && movementInput.x > 0){
                                    if(horizontalVelocity.x < -airSpeed){ //Prevent exploit by snapping to recover knockback
                                        horizontalVelocity.x = -airSpeed; //Simply return player to max air speed after jumping
                                    }
                                    horizontalVelocity.x = Mathf.Abs(horizontalVelocity.x);
                                }
                                if(horizontalVelocity.x > 0 && movementInput.x < 0){
                                    if(horizontalVelocity.x > airSpeed){
                                        horizontalVelocity.x = airSpeed;
                                    }
                                    horizontalVelocity.x = horizontalVelocity.x - (horizontalVelocity.x * 2);
                                }
                            }else{
                                horizontalVelocity.x /= 2f;
                        }
                        anim.SetTrigger("Jump");
                    }else{// We're not on the ground, check if we can jump again.
                        if(jumpsMade < maxJumps){
                            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                            jumpsMade += 1;
                            //Debug.Log("Air Jump");
                            if(isJumpingSnapped){
                                if(horizontalVelocity.x < 0 && movementInput.x > 0){
                                    if(horizontalVelocity.x < -airSpeed){ //Prevent exploit by snapping to recover knockback
                                        horizontalVelocity.x = -airSpeed; //Simply return player to max air speed after jumping
                                    }
                                    horizontalVelocity.x = (Mathf.Abs(horizontalVelocity.x)) / 2f;
                                }
                                if(horizontalVelocity.x > 0 && movementInput.x < 0){
                                    if(horizontalVelocity.x > airSpeed){
                                        horizontalVelocity.x = airSpeed;
                                    }
                                    horizontalVelocity.x = (horizontalVelocity.x - (horizontalVelocity.x * 2f)) / 2f;
                                }
                            }else{
                                horizontalVelocity.x /= 2f;
                            }
                            anim.SetTrigger("Jump");
                        }
                    }
                }
                nextJumpTime = Time.time +(1f / jumpRate);
            }
        }
    }

    //Coroutine for hitlag
    IEnumerator Hitlag(float d){
        hitlag = true;
        anim.speed = 0f;
        yield return new WaitForSeconds((d * 0.65f + 6f) * Time.fixedDeltaTime);
        hitlag = false;
        anim.speed = 1f;
    }

    //Couroutine for targetted player hitlag. Delivers knockback after hitlag.
    internal IEnumerator HitlagTarget(float d, float h, float v, float o, float hitstun){
        Debug.Log("Hitstun: " + hitstun);
        hitlag = true;
        if(isShielding){
            DamageShield(d);
        }
        yield return new WaitForSeconds((d * 0.65f + 6f) * Time.fixedDeltaTime);
        if(!isShielding){
            if(hitstun > 32){ //explosive launch
                float horizLaunch = h * o;
                float vertLaunch = v;
                transform.position = new Vector3(transform.position.x + horizLaunch, transform.position.y + vertLaunch, transform.position.z);
            }else{
                horizontalVelocity = Vector3.right * h * o;
                verticalVelocity = Vector3.up * v;
            }
            jumpsMade = 0;
        }
        hitlag = false;
    }

    internal IEnumerator Hitstun(float h){
        if(!isShielding){
            anim.SetTrigger("HitStun");
        }
            hitstun = true;
            float t = h * Time.fixedDeltaTime;
            yield return new WaitForSeconds(t);
            hitstun = false;
    }

    //Detecting triggers, usually for edge grabbing
    private void OnTriggerEnter(Collider other) {
        PlatformEdge edge = other.GetComponent<PlatformEdge>(); //check if its an edge
        if(edge != null){ //do edge grab
            Vector3 triggerPoint = other.transform.position;
            horizontalVelocity.x = 0f;
            verticalVelocity.y = 0f;
            if(!isGrounded){ //We are falling and we touched an edge
                Vector3 edgePoint = edge.edgePoint;
                if(other.gameObject.layer == 15){ //index for left edge layer
                    EdgeGrab(edgePoint, triggerPoint, -1);
                }else if(other.gameObject.layer == 16){ //index for right edge layer
                    EdgeGrab(edgePoint, triggerPoint, 1);
                }
            }
        }
    }
    public void CompleteAttackLag(){
        isAttacking = false;
        attackLag = false;
    }
    void OnCollisionEnter(){
        Debug.Log("Collided");
    }

    void EdgeGrab(Vector3 edgePoint, Vector3 triggerPoint, int orient){
        if(!hasIntangibleEdge){
            StartCoroutine(EdgeGrabIntangibility(airTime));
        }
        if(isShielding){
            DisableShield();
        }
        jumpsMade = 0;
        isAttacking = false;
        isEdgeGrab = true;
        anim.SetTrigger("EdgeGrab");
        EdgeGrabOrient = orient;
        //Specify the grab location according to character radius and height.
        Vector3 grabLocation = new Vector3(triggerPoint.x + (edgePoint.x + (charRadius * orient)), triggerPoint.y + edgePoint.y - (charHeight / 2), transform.position.z);
        transform.position = grabLocation;
    }

    IEnumerator EdgeGrabIntangibility(float air){
        hasIntangibleEdge = true;
        float a = Mathf.Clamp(air, 0, 300);
        float p = Mathf.Clamp(percentage, 0, 120);
        float f = (a * 0.2f + 64f) - (p * 44f / 120f); //Smash Ultimate calculation for intangibility time
        float t = f * Time.fixedDeltaTime;
        EnableIntangibility();
        yield return new WaitForSeconds(t);
        DisableIntangibility();
    }

    void spawnStepParticle(){
        if(isGrounded && isAlive){
            Vector3 orient = Vector3.zero;
            if(horizontalVelocity.x > 0){
                orient = Vector3.zero;
            }else if(horizontalVelocity.x < 0){
                orient = new Vector3(0f ,180f, 0f);
            }
            particle.SpawnStepParticle(transform.position, orient);
        }
    }
    void spawnDashParticle(){
        if(isAlive){
            Vector3 orient = Vector3.zero;
            if(horizontalVelocity.x > 0){
                orient = Vector3.zero;
            }else if(horizontalVelocity.x < 0){
                orient = new Vector3(0f ,180f, 0f);
            }
            particle.SpawnDashParticle(transform.position, orient);
        }
    }
    void spawnLandParticle(){
        if(isAlive){
            particle.SpawnLandParticle(transform.position, Vector3.zero);
        }
    }
    void SpawnHeavyBlastParticle(Vector3 position, Vector3 rotation){
        particle.SpawnHeavyBlastParticle(position, rotation);
    }
    void SpawnLightBlastParticle(Vector3 position, Vector3 rotation){
        particle.SpawnLightBlastParticle(position, rotation);
    }
    void EnableIntangibility(){
        isIntangible = true;
        StartFlicker(8, Color.white);
    }
    void DisableIntangibility(){
        isIntangible = false;
        StopFlicker();
    }
    void StartFlicker(float speed, Color color){
        sprite.material.SetFloat("_Flicker", 1);
        sprite.material.SetFloat("_FlickerSpeed", speed);
        sprite.material.SetColor("_FlickerColor", color);
    }
    void StopFlicker(){
        sprite.material.SetFloat("_Flicker", 0);
    }
    void EnableShield(){
        shield.EnableShield();
        isDashed = false;
        isShielding = true;
    }
    void DisableShield(){
        shield.DisableShield();
        isShielding = false;
    }
    void DamageShield(float d){
        shield.DamageShield(d);
        Debug.Log("Damaged shield" + playerNumber + " by " + d);
    }
    internal void ShieldBreak(){
        isShielding = false;
        isStunned = true;
        StartFlicker(2, Color.red);
        spawnLandParticle();
        verticalVelocity.y = 30f;
        float stunTime = ((400 - percentage) + 60) * Time.fixedDeltaTime;
        stunCoroutine = StartCoroutine(Stun(stunTime));
    }
    internal void StopStun(){
        if(stunCoroutine != null){
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
            isStunned = false;
            StopFlicker();
        }
    }
    IEnumerator Stun(float t){
        yield return new WaitForSeconds(t);
        isStunned = false;
        StopFlicker();
    }
    IEnumerator Dodge(float t){
        EnableIntangibility();
        isDodging = true;
        yield return new WaitForSeconds(t);
        DisableIntangibility();
        isDodging = false;
    }
    void SpotDodge(){
        StartCoroutine(Dodge(spotDodgeTime));
    }
    void AirDodge(){
        StartCoroutine(Dodge(spotDodgeTime));
        jumpsMade = 10;
        if(x > 0){ //Directional airdodge
            verticalVelocity.y = 0f;
            horizontalVelocity.x = airSpeed;
        }else if(x < 0){
            verticalVelocity.y = 0f;
            horizontalVelocity.x = -airSpeed;
        }
        hasAirDodge = true;
    }

    internal void Knockout(){
        sprite.enabled = false;
        isDashed = false;
        isFastFalling = false;
    }
    internal void Respawn(){
        isAlive = true;
        sprite.enabled = true;
        horizontalVelocity = Vector3.zero;
        verticalVelocity = Vector3.zero;
        percentage = 0f;
        if(OnRespawn != null){
            OnRespawn();
        }
    }

    //Draw gizmos to ease editor production
    void OnDrawGizmosSelected() {
        if(attackPoint == null || characterID == null){
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, characterID.attackRange);
    }

    internal void SetVelocity(float x, float y){ //what to do with this? may need improvements in functionality
        horizontalVelocity.x = x;
        verticalVelocity.y = y;
    }
}
