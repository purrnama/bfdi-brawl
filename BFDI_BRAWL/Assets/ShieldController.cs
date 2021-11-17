using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    private GlobalFunctions globalFunctions = null;
    PlayerMovement controller;
    Character_ID id;
    [SerializeField] private float durationTime = 0f;
    float currentTime;
    Material material;
    public bool isShielding = false;
    float maxSize, minSize, maxHP, currentHP, resetBroken;
    float deplete, regen;
    float step;

    void Start()
    {
        controller = transform.parent.GetComponent<PlayerMovement>();
        globalFunctions = controller.globalFunctions;
        id = controller.characterID;
        maxSize = id.shieldMaxSize;
        minSize = id.shieldMinSize;
        maxHP = globalFunctions.ShieldMaxHealth;
        currentHP = maxHP;
        resetBroken = globalFunctions.ShieldBrokenReset;
        deplete = globalFunctions.ShieldDepletionRate;
        regen = globalFunctions.ShieldRegenerationRate;
        material = GetComponent<MeshRenderer>().material;
        material.SetFloat("_DissolveStep", 0f);
    }

    void Update()
    {
        if(isShielding){
            if(currentTime <= durationTime){
                currentTime += Time.deltaTime;
                step = Mathf.Lerp(0, 1, currentTime / durationTime);
                material.SetFloat("_DissolveStep", step);
            }
            if(currentHP > 0f){
                currentHP -= (deplete * Time.deltaTime);
            }else{
                ShieldBreak();
            }
        }else{
            if(currentTime <= durationTime){
                currentTime += Time.deltaTime;
                step = Mathf.Lerp(1, 0, currentTime / durationTime);
                material.SetFloat("_DissolveStep", step);
            }
            if(currentHP < maxHP){
                currentHP += (regen * Time.deltaTime);
            }
        }
        UpdateSize();
    }
    public void EnableShield(){
        if(currentHP > 0){
            isShielding = true;
            currentTime = 0f;
        }
    }
    public void DisableShield(){
        isShielding = false;
        currentTime = 0f;
    }
    public void DamageShield(float damage){
        currentHP -= damage;
        StartCoroutine(DamageTransition());
    }
    void ShieldBreak(){
        controller.ShieldBreak();
        DisableShield();
        currentHP = resetBroken;
    }

    void UpdateSize(){
        float normalHP = Mathf.Lerp(0, 1, currentHP/maxHP); //normalize currentHP
        float newSize = Mathf.Lerp(minSize, maxSize, normalHP); //use normalizedHP to scale shield;
        transform.localScale = new Vector3(newSize, newSize, newSize);
    }
    IEnumerator DamageTransition(){
        material.SetFloat("_DisplaceStrength", 1f);
        yield return new WaitForSeconds(0.1f);
        material.SetFloat("_DisplaceStrength", 0f);
    }
}
