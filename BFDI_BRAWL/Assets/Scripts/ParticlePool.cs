using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    [SerializeField] private GameObject dashParticle = null;
    [SerializeField] private GameObject stepParticle = null;
    [SerializeField] private GameObject landParticle = null;
    [SerializeField] private GameObject lightBlastParticle = null;
    [SerializeField] private GameObject heavyBlastParticle = null;
    // Start is called before the first frame update
    public void SpawnDashParticle(Vector3 position, Vector3 rotation){
        Quaternion eulerRotation = Quaternion.Euler(rotation);
        GameObject dash = Instantiate(dashParticle, position, eulerRotation, gameObject.transform);
    }
    public void SpawnStepParticle(Vector3 position, Vector3 rotation){
        Quaternion eulerRotation = Quaternion.Euler(rotation);
        GameObject step = Instantiate(stepParticle, position, eulerRotation, gameObject.transform);
    }
    public void SpawnLandParticle(Vector3 position, Vector3 rotation){
        Quaternion eulerRotation = Quaternion.Euler(rotation);
        GameObject land = Instantiate(landParticle, position, eulerRotation, gameObject.transform);
    }
    public void SpawnLightBlastParticle(Vector3 position, Vector3 rotation){
        Quaternion eulerRotation = Quaternion.Euler(rotation);
        GameObject blast = Instantiate(lightBlastParticle, position, eulerRotation, gameObject.transform);
    }
    public void SpawnHeavyBlastParticle(Vector3 position, Vector3 rotation){
        Quaternion eulerRotation = Quaternion.Euler(rotation);
        GameObject blast = Instantiate(heavyBlastParticle, position, eulerRotation, gameObject.transform);
    }
}
