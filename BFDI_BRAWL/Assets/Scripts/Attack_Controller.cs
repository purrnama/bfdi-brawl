
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Attack_Controller : MonoBehaviour
{
    PlayerMovement player;
    internal Moveset moveset;
    internal event Action OnDamage;
    void Start(){
        player = GetComponent<PlayerMovement>();
    }
    public void Attack(InputAction.CallbackContext context){
        if(context.performed){
            if(player.active && !player.hitlag && !player.isDodging && !player.isShielding && !player.isStunned && !player.attackLag && player.isAlive){
                if(Time.time >= player.nextAttackTime){
                    InitiateAttack();
                    player.nextAttackTime = Time.time + (1f / player.attackRate);
                }
            }
        }
    }
    public void Special(InputAction.CallbackContext context){
        if(context.performed){
            if(player.active && !player.hitlag && !player.isDodging && !player.isShielding && !player.isStunned && !player.attackLag && player.isAlive){
                if(Time.time >= player.nextAttackTime){
                    InitiateSpecial();
                    player.nextAttackTime = Time.time + (1f / player.attackRate);
                }
            }
        }
    }
    public void Grab(InputAction.CallbackContext context){
        if(context.performed){
            if(player.active && !player.hitlag && !player.isDodging && !player.isShielding && !player.isStunned && !player.attackLag && player.isAlive){
            }
        }
    }

    //Based on current instance of direction and input, calculate which attack to deliver.
    Vector2 CalculateAttackOrientation(Vector3 attackPosition){
        Vector3 point = attackPosition - player.controllerCenter;
        Vector2 returnState = Vector2.zero;
        int horiz = 0;
        int vert = 0;
        if(point.x > 0){
            horiz = 1;
        }else if(point.x < 0){
            horiz = -1;
        }
        if(point.y > 0){
            vert = 1;
        }else if(point.y < 0){
            vert = -1;
        }else{
            vert = 0;
        }
        returnState = new Vector2(horiz, vert);
        //Debug.Log("Calculate attack orientation: " + returnState);
        return returnState;
    }
    //Deliver the attack based on the current orientation.
    void AttackType(bool isNormal, Vector2 state, Vector2 input, bool ground, bool dash){
        int direction = 0;
        Vector3 attackType = Vector3.zero;
        moveset.state = state;
        //Figure out the direction based on attack state
        if(state.x == 0 && state.y == 1){
            direction = 1;
        }else if(state.x != 0 && state.y == 1){
            direction = 2;
        }else if(state.x != 0 && state.y == 0){
            direction = 3;
        }else if(state.x != 0 && state.y == -1){
            direction = 4;
        }else if(state.x == 0 && state.y == -1){
            direction = 5;
        }
        //Based on direction detected, check with multiple conditions, and call from moveset
        if(direction == 1){
            if(ground){//UNeutral
                if(isNormal){
                    attackType = player.uNeutral;
                    moveset.UNeutral();
                }else{//USpecial
                    moveset.Special();
                }
            }else{//UAir
                if(isNormal){
                    attackType = player.uAir;
                    moveset.UAir();
                }else{//USpecial
                    moveset.Special();
                }
            }
        }else if(direction == 2){//UTilt
            if(!ground){
                if(isNormal){
                    attackType = player.uTilt;
                    moveset.UTilt();
                }else{//USpecial
                    moveset.Special();
                }
            }else{
                if(isNormal){
                    attackType = player.fNeutral;
                    moveset.FNeutral();
                }else{//USpecial
                    moveset.Special();
                }
            }
        }else if(direction == 3){
            if(ground){
                if(isNormal){
                    if(dash){//FDash
                        attackType = player.fDash;
                        moveset.FDash();
                    }else if(input.x == 0){//FNeutral
                        attackType = player.fNeutral;
                        moveset.Jab();
                    }else if((state.x > 0 && input.x > 0) || (state.x < 0 || input.x < 0)){//FSmash
                        attackType = player.fSmash;
                        moveset.FNeutral();
                    }
                }else{
                    if(input.x == 0){//Special
                        moveset.Special();
                    }else if((state.x > 0 && input.x > 0) || (state.x < 0 || input.x < 0)){//FSpecial
                        moveset.Special();
                    }
                }
            }else{//FAir
                if(isNormal){
                    attackType = player.fAir;
                    moveset.FAir();
                }else{//FSpecial
                    moveset.Special();
                }
            }
        }else if(direction == 4){//DTilt
            if(isNormal){
                attackType = player.dTilt;
                moveset.DTilt();
            }else{//DSpecial
                moveset.Special();
            }
        }else if(direction == 5){
            if(ground){//DNeutral
                if(isNormal){
                    attackType = player.dNeutral;
                    moveset.DNeutral();
                }else{//DSpecial
                    moveset.Special();
                }
            }else{//DAir
                if(isNormal){
                    attackType = player.dAir;
                    moveset.DAir();
                }else{//DSpecial
                    moveset.Special();
                }
            }
        }
        //Debug.Log("Delivering Attack: " + attackType);
    }
    Vector3 UpdateAttackPosition(){
        Vector3 attackPosition;
        Vector2 attackState = new Vector2(player.x, player.y);
        Vector2 newPoint = Vector2.zero;

        if(attackState.x > 0){
            newPoint.x = player.attackDistance.x + player.controllerCenter.x;
        }else if(attackState.x < 0){
            newPoint.x = -player.attackDistance.x + player.controllerCenter.x;
        }else{
            newPoint.x = player.controllerCenter.x;
        }

        if(attackState.y > 0){
            newPoint.y = player.attackDistance.y + player.controllerCenter.y;
        }else if(attackState.y < 0){
            newPoint.y = -player.attackDistance.y + player.controllerCenter.y;
        }else{
            newPoint.y = player.controllerCenter.y;
        }

        if(attackState.x == 0 && attackState.y == 0){
            if(!player.sprite.flipX){
                newPoint.x = player.attackDistance.x + player.controllerCenter.x;
            }else{
                newPoint.x = -player.attackDistance.x + player.controllerCenter.x;
            }
        }
        attackPosition = new Vector3(newPoint.x, newPoint.y, 0f);
        return attackPosition;
    }
    //Called by the movement controller when the attack key is pressed.
    internal void InitiateAttack(){
        player.isAttacking = true;
        Vector3 attackPosition = UpdateAttackPosition();
        player.attackPoint.localPosition = attackPosition;
        Vector2 state = CalculateAttackOrientation(attackPosition);
        Vector2 persistInput = new Vector2(player.x, player.y);
        bool persistGrounded = player.isGrounded;
        bool persistDash = player.isDashed;
        bool inWindow = player.isReadAction;

        //compare data
        string type = null;
        if(inWindow){
            type = "Smash";
        }else{
            type = "Normal";
        }

        AttackType(true, state, persistInput, persistGrounded, persistDash);
        Debug.Log(type);
    }
    internal void InitiateSpecial(){
        //Debug.Log("Speciale needs");
        Vector3 attackPosition = UpdateAttackPosition();
        player.attackPoint.localPosition = attackPosition;
        Vector2 state = CalculateAttackOrientation(attackPosition);
        Vector2 persistInput = new Vector2(player.x, player.y);
        bool persistGrounded = player.isGrounded;
        bool persistDash = player.isDashed;

        AttackType(false, state, persistInput, persistGrounded, persistDash);
    }

    internal void FireProjectile(GameObject prefab){

        GameObject spawn = Instantiate(prefab, player.attackPoint.position, Quaternion.identity);
        Projectile projectile = spawn.GetComponent<Projectile>();
        Vector3 attackPosition = UpdateAttackPosition();
        player.attackPoint.localPosition = attackPosition;
        Vector2 orient = CalculateAttackOrientation(attackPosition);
        projectile.owner = player;
        projectile.gravity = 0f;
        if(orient.x > 0){
            projectile.horizontalVelocity.x = 18f;
        }else{
            projectile.horizontalVelocity.x = -18f;
        }
        projectile.verticalVelocity.y = 0f;
        spawn.SetActive(true);

        Debug.Log("Fire Projectile");
    }

    //Returns a knockback value if an attack is delivered. Based on SSB formula.
    float KnockbackValue(float percentage, float damage, float weight, float scaling, float baseknockback){
        return ((((((percentage / 10f) + ((percentage * damage) / 20f)) * (200f/ (weight+100)) * 1.4f) + 18f) * scaling) + baseknockback);
    }
    void SpawnHeavyBlastParticle(Vector3 position, Vector3 rotation){
        player.particle.SpawnHeavyBlastParticle(position, rotation);
    }
    void SpawnLightBlastParticle(Vector3 position, Vector3 rotation){
        player.particle.SpawnLightBlastParticle(position, rotation);
    }

    //Plays the appropriate attack sounds
    AudioClip HitSound(int type){
        AudioClip[] retrieveSound = null;
        if(type == 1){
            retrieveSound = player.globalFunctions.lightHitSounds;
        }
        if(type == 2){
            retrieveSound = player.globalFunctions.heavyHitSounds;
        }
        if(type == 3){
            retrieveSound = player.globalFunctions.smashHitSounds;
        }
        int randomize = (int)UnityEngine.Random.Range(0, retrieveSound.Length - 1);
        return retrieveSound[randomize];
    }

    //Coroutine for hitlag
    IEnumerator Hitlag(float d){
        player.hitlag = true;
        player.anim.speed = 0f;
        yield return new WaitForSeconds((d * 0.65f + 6f) * Time.fixedDeltaTime);
        player.hitlag = false;
        player.anim.speed = 1f;
    }

    //Called by the attacker to damage the opponent.
    void Damage(float angle, float knockback, int orient, float damage){
        if (OnDamage != null) {
            OnDamage();
        }
        float radAngle = angle * Mathf.PI / 180;
        float horizV = knockback * Mathf.Sin(radAngle);
        float vertV = knockback * Mathf.Cos(radAngle);

        float hitstunValue = 32f + (0.1455f * (knockback - 82.5f));
        player.hasIntangibleEdge = false;

        player.StopStun();
        player.CancelCharge();
        StartCoroutine(player.Hitstun(hitstunValue));
        StartCoroutine(player.HitlagTarget(damage, horizV, vertV, orient, hitstunValue));
        //Debug.Log(angle + ", " + knockback + ", " + orient + ", " + (hitstunValue * Time.deltaTime));
    }

    //Create a sphere hitbox at a direction to detect opponents within the attack range of the player.
    //type.x = knockback, type.y = angle, type.z = scaling
    internal void MeleeHit(Vector2 state, Vector3 type){
        player.attackLag = true;
        Collider[] hitOpponents = Physics.OverlapSphere(player.attackPoint.position, player.attackRange, player.opponentLayers);

        //If hit opponent, apply hitlag
        if(hitOpponents.Length > 0){
            StartCoroutine(Hitlag(type.x));
        }
        //For every opponent within area of attack range, damage them.
        foreach(Collider opponent in hitOpponents){
            PlayerMovement opp = opponent.GetComponent<PlayerMovement>();
            Vector3 effectAngle = new Vector3(0,0, (-type.y * state.x)); //get angle for blast effects
            float knockback = KnockbackValue(opp.percentage, type.x, opp.weight, (type.z / 100), type.x);
            Debug.Log("Knockback: " + knockback);

            if(!opp.isShielding && !opp.isIntangible){
                opp.percentage += type.x; //Add percentage first
            }
            if(!opp.isIntangible){
                if(knockback > 60){
                    SpawnHeavyBlastParticle(transform.position, effectAngle);
                    player.sound.PlayOneShot(HitSound(2));
                }else{
                    SpawnLightBlastParticle(transform.position, effectAngle);
                    player.sound.PlayOneShot(HitSound(1));
                }
                opp.attack.Damage(type.y, knockback, (int)state.x, type.x);
            }
        }
    }
}
