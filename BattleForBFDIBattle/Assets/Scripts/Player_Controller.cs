using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour {

    GameManager manager;
    public int playerNumber;
	public Character_ID characterID;
    public float limitMoveOnAttack;
    public float doubleKeySetTime;
    float doubleKeyTime = 0;
    int axisPresses;
    GameObject attackTrigger;
    public GameObject stat, indicator;
    BoxCollider trigger;
    GameObject parent, dashParticle, diveParticle, impactParticle, lavaSplash, blockBubble;
    public GameObject deathConfetti;
    public Transform effectStash;
    Camera_Controller cam;
    float knockback = 20;
    float stunChance;
    public float damagePercent = 0;

	bool jumped, doublejumped;
    public bool blocking;
    bool dived;
    public bool mobility = true;
    public bool flipped;
    bool limitMove;
    bool dashed;
    public bool debugDisable;

	Rigidbody rb;
    SpriteRenderer sprite;
	CapsuleCollider playerCollider;
	Vector3 moveDirection;
    Animator anim;
    AudioSource sound;
    Vector3 impactForce;
    float impactX, impactY, impactZ;

    string[] inputs = {"Horizontal","Vertical","Attack","Special", "Block"};

	//FLIPPED = FACING RIGHT

	void Awake () {
		InitializeVariables();
	}

    void InitializeVariables(){
        
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera_Controller>();
		rb = GetComponent<Rigidbody>();
		playerCollider = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();
        anim.runtimeAnimatorController = characterID.animatorController;
        attackTrigger = transform.GetChild(0).gameObject;
        trigger = attackTrigger.GetComponent<BoxCollider>();
        parent = transform.parent.parent.gameObject;
        effectStash = parent.transform.Find("EffectsStash");
        dashParticle = parent.transform.Find("DashSmoke").gameObject;
        diveParticle = parent.transform.Find("DiveSmoke").gameObject;
        blockBubble = parent.transform.Find("BlockBubble" + playerNumber).gameObject;
        impactParticle = parent.transform.Find("ImpactDebris").gameObject;
        lavaSplash = parent.transform.Find("LavaSplash").gameObject;
        deathConfetti = parent.transform.Find("DeathConfetti").gameObject;
        stat = GameObject.Find("Stats" + playerNumber);
        indicator = GameObject.Find("Indicator" + playerNumber);
        indicator.GetComponent<Follow_Player>().offset = characterID.indicatorOffset;
        blockBubble.GetComponent<Block_Controller>().playerRef = transform;
        blockBubble.GetComponent<Follow_Player>().offset = characterID.blockOffset;
        blockBubble.transform.localScale = characterID.blockResize;
        sound = GetComponent<AudioSource>();

        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = inputs[i] + playerNumber.ToString();
        }
    }

	void Start(){

		playerCollider.material = characterID.playerPhysic;
		rb.mass = characterID.mass;
		rb.drag = characterID.drag;
        sprite = GetComponent<SpriteRenderer>();

	}
	
	// Update is called once per frame
	void Update () {

		if(characterID == null){

			Debug.LogError("No CharacterID you dummy!");
            return;
		}
        //--------------------------
        if(mobility && !debugDisable){

        float horizontalMovement = Input.GetAxis(inputs[0]);

        if (limitMove == false)
        {
            moveDirection = (horizontalMovement * transform.right);
        }else{
            moveDirection = (horizontalMovement * transform.right / limitMoveOnAttack);
        }

        if(dashed){
            moveDirection.x = moveDirection.x * characterID.dashPower;
        }

        if(Input.GetButtonDown(inputs[0])){
            if(axisPresses == 0){
                axisPresses = 1;
            }
            else if(axisPresses == 1){
                axisPresses = 2;
            }
        }
        else if(axisPresses == 2){

            Dash();
            axisPresses = 0;
            doubleKeyTime = 0;

        }

        if(axisPresses == 1){

            doubleKeyTime += Time.deltaTime;

        }
        if(doubleKeyTime > doubleKeySetTime){

            axisPresses = 0;
            doubleKeyTime = 0;

        }
        

        anim.SetInteger("MoveHorizontal", Mathf.RoundToInt(horizontalMovement));

        //--------------------------
        if(mobility){
        if(horizontalMovement < 0f){
            flipped = false;
        }else if (horizontalMovement > 0f){
            flipped = true;
        }
        }

        //--------------------------

        if(flipped){
            sprite.flipX = true;
        }else{
            sprite.flipX = false;
        }

        //--------------------------

        if (Input.GetButtonDown(inputs[2]))
        {
            Attack();
        }

        //--------------------------

        if (Input.GetButtonDown(inputs[3]))
        {
            Special();
        }
        if(Input.GetButton(inputs[4]))
        {
            if(!blocking){
            Block();
            }
        }
        if(Input.GetButtonUp(inputs[4]))
        {
            UnBlock();
        }

        if(Input.GetButtonDown(inputs[1])){
            if(Input.GetAxisRaw(inputs[1]) > 0){
                Jump();
            }else if(Input.GetAxisRaw(inputs[1]) < 0){
                Duck();
            }
        }

        }

        if(impactForce.x != 0f){
            impactForce.x = Mathf.SmoothDamp(impactForce.x, 0f, ref impactX, 0.3f);
        }
        if(impactForce.y != 0f){
            impactForce.y = Mathf.SmoothDamp(impactForce.y, 0f, ref impactY, 0.3f);
        }


	}

	void FixedUpdate () {

		Move();

	}

	void Move(){

        Vector3 VelFix = new Vector3(rb.velocity.x / 2, rb.velocity.y, 0f);
        rb.velocity = moveDirection * characterID.speed * Time.deltaTime;
        rb.velocity += VelFix + impactForce;

	}

	void Jump(){

        if(mobility){
		    if(!jumped){

                Debug.Log("Jumped");
		    	jumped = true;
                doublejumped = false;
                rb.velocity = Vector3.zero;
		    	rb.AddForce(Vector3.up * characterID.jumpPower, ForceMode.Impulse);
                anim.SetTrigger("Jump");
                anim.SetBool("Grounded", false);
                PlayLocalSound("jump", false);
                return;

		    }else if (!doublejumped){
                Debug.Log("Double Jumped");
                jumped = true;
                doublejumped = true;
                rb.velocity = Vector3.zero;
		    	rb.AddForce(Vector3.up * characterID.jumpPower, ForceMode.Impulse);
                anim.SetTrigger("DoubleJump");
                anim.SetBool("Grounded", false);
                PlayLocalSound("jump", false);
                return;
            }
        }

	}

    void Attack(){

        anim.SetTrigger("Attack");

    }

    void Special()
    {

        anim.SetTrigger("Special");

    }

    void Block(){
        if(!jumped && blockBubble.GetComponent<Block_Controller>().currentCooldown > 3f){
        mobility = false;
        blocking = true;
        PlayLocalSound("block", false);
        }

    }
    public void UnBlock(){

        mobility = true;
        blocking = false;
        blockBubble.GetComponent<Block_Controller>().anim.SetTrigger("Exit");

    }
    public void BlockStun(){
        anim.SetTrigger("Stun");
        mobility = false;
        PlayLocalSound("hurtheavy", true);
    }

    void Duck(){

        if(jumped && !dived){
            rb.AddForce(Vector3.down * characterID.divePower, ForceMode.Impulse);
            GameObject DiveSmoke = Instantiate(diveParticle, transform.position, diveParticle.transform.rotation, transform);
            DiveSmoke.SetActive(true);
            dived = true;
            anim.SetTrigger("Dive");
        }

    }
    void Dash(){

        dashed = true;
        GameObject DashSmoke = Instantiate(dashParticle, transform.position, Quaternion.identity, effectStash);
        DashSmoke.SetActive(true);
        StartCoroutine(DashPeriod());

    }

	void OnCollisionEnter(Collision c){
        
        jumped = false;
        doublejumped = false;
        dived = false;
        anim.SetBool("Grounded", true);

	}

    private void OnTriggerEnter(Collider c){

        if(c.gameObject.tag == "AttackTrigger"){

            bool heaviness = c.gameObject.GetComponent<AttackTriggerController>().heavy;
            bool flipped = c.gameObject.GetComponent<AttackTriggerController>().flipped;
            float velX = c.transform.parent.GetComponent<Rigidbody>().velocity.x;
            float velY = c.transform.parent.GetComponent<Rigidbody>().velocity.y;
            int hitPlayer = c.transform.parent.GetComponent<Player_Controller>().playerNumber;
            Hurt(heaviness, flipped, velX, velY, hitPlayer);

        }
        
        if(c.gameObject.tag == "Hazard"){

            Debug.Log("Ouch! A hazard!");
            GameObject LavaSplash = Instantiate(lavaSplash, transform.position, lavaSplash.transform.rotation, effectStash);
            LavaSplash.SetActive(true);

            manager.HazardDeath(this);

        }

    }

    public void LimitMoveOnAttack(){

        limitMove = true;

    }

    public void FreeMovement(){

        limitMove = false;

    }

    public void Immobilize(){

        mobility = false;

    }

    public void Mobilize(){

        mobility = true;

    }

    public void ImpactBounce(){

		rb.AddForce(Vector3.up * characterID.jumpPower / 1.5f, ForceMode.Impulse);
        GameObject impact = Instantiate(impactParticle, transform.position, impactParticle.transform.rotation, effectStash);
        impact.SetActive(true);

    }

    public void EnableLightAttackTrigger(){

        attackTrigger.GetComponent<AttackTriggerController>().heavy = false;
        trigger.enabled = true;
        PlayLocalSound("attack", true);

    }

    public void DisableAttackTrigger(){

        trigger.enabled = false;

    }

    public void EnableHeavyAttackTrigger(){

        attackTrigger.GetComponent<AttackTriggerController>().heavy = true;
        trigger.enabled = true;
        PlayLocalSound("special", true);

    }

    void Hurt(bool heavy, bool flipped, float velX, float velY, int attackingPlayerNumber){
    if(!blocking){
        float damageDone = 0f;
        float multiplyBySpeed = (Mathf.Abs(velX) + Mathf.Abs(velY) / 2) / 10;

        if(multiplyBySpeed < 1){
        if(!heavy){
            multiplyBySpeed = 0.1f;
        }else{
            multiplyBySpeed = 1f;
        }
        }
        multiplyBySpeed = Mathf.Clamp(multiplyBySpeed, 0f, 1.5f);
        if(!heavy){
        cam.SmolShake();
        damagePercent += (1f + multiplyBySpeed);
        stunChance = 1;
        }else{
        cam.BigShaq();
        damagePercent += (3f + multiplyBySpeed);
        stunChance = 30;
        }


        stunChance += multiplyBySpeed + (damagePercent / 10);
        damageDone = damagePercent / 50f * multiplyBySpeed;
        Debug.Log("This is Player " + attackingPlayerNumber + " attacking Player " + playerNumber + ". Damage done = " + damageDone + ", Multiply by speed: " + multiplyBySpeed);

        if(!jumped){
        float randomizeStun = Random.Range(0,100);
        if(randomizeStun <= stunChance){
            anim.SetBool("Grounded", false);
            anim.SetTrigger("Stun");
            mobility = false;
            PlayLocalSound("hurtheavy", true);
            PlayGlobalSound("hitheavy");
        }else{
            anim.SetTrigger("Hurt");
            PlayLocalSound("hurtlight", true);
            PlayGlobalSound("hitlight");
        }
        }else{
            anim.SetBool("Grounded", false);
            anim.SetTrigger("Stun");
            mobility = false;
            PlayLocalSound("hurtheavy", true);
            PlayGlobalSound("hitheavy");
        }

        if(flipped){
            impactForce = new Vector3(1f,0.1f,0f) * knockback * damageDone;
        }else{
            impactForce = new Vector3(-1f,0.1f,0f) * knockback * damageDone;
        }
        manager.UpdateDamage(playerNumber);

    }else{
        impactForce = Vector3.zero;
        blockBubble.GetComponent<Block_Controller>().Damaged();
    }

    }

    IEnumerator DashPeriod(){

        yield return new WaitForSeconds(characterID.dashPeriod);

        dashed = false;

    }

    public void PlayLocalSound(string type, bool silentChance){
        //Sound from Character_ID
        type = type + "Sounds";
        AudioClip[] soundType = (AudioClip[])characterID.GetType().GetField(type).GetValue(characterID);
        if(soundType.Length == 0){
            Debug.LogWarning("Requested soundType is empty. You are looking for: " + type + ". Check if this is the correct type or add some sound files.");
            return;
        }else{

            int selectRandom;

            if(silentChance){
            selectRandom = Random.Range(-1,soundType.Length);
            }else{
            selectRandom = Random.Range(0,soundType.Length);
            }

            if(selectRandom == -1){
            return;
            }else{
            AudioClip pickedSound = soundType[selectRandom];
            sound.PlayOneShot(pickedSound);
            }
        }
    }
    void PlayGlobalSound(string type){
        //Sound from Game Manager
        type = type + "Sounds";
        AudioClip[] soundType = (AudioClip[])manager.GetType().GetField(type).GetValue(manager);
        if(soundType.Length == 0){
            Debug.LogWarning("Requested soundType is empty. You are looking for: " + type + ". Check if this is the correct type or add some sound files.");
            return;
        }else{

            int selectRandom;

            selectRandom = Random.Range(0,soundType.Length);

            AudioClip pickedSound = soundType[selectRandom];
            sound.PlayOneShot(pickedSound);
        }
    }

}
