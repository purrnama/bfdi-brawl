using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testParticleEmission : MonoBehaviour
{
    ParticleSystem particle;
    bool blastEnabled = false;
    
    // Start is called before the first frame update
    void Start()
    {
        particle = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)){
            if(particle.isEmitting){
                particle.Stop();
            }else{
                particle.Play();
            }
        }
    }
}
