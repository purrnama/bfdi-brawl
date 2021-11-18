using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mock_Moveset : Moveset
{
    GameObject projectile;

    void Start(){
        projectile = id.CharacterAssets.Find(item => item.name == "TestProjectile");
    }
    //Normal ------------------------------------------------
    public override void UNeutral(){
        player.anim.SetTrigger("UNeutral");
        attack.MeleeHit(state, player.uNeutral);
    }
    public override void UAir(){
        player.SetVelocity(0, 20);
        player.anim.SetTrigger("UAir");
        attack.MeleeHit(state, player.uAir);
    }
    public override void USmash(){
        player.anim.SetTrigger("USmash");
        attack.MeleeHit(state, player.uSmash);
    }
    public override void UTilt(){
        player.anim.SetTrigger("UTilt");
        attack.MeleeHit(state, player.uTilt);
    }
    public override void Jab(){
        player.anim.SetTrigger("FNeutral");
        attack.MeleeHit(state, player.jab);
    }
    public override void FNeutral(){
        player.anim.SetTrigger("FNeutral");
        attack.MeleeHit(state, player.fNeutral);
    }
    public override void FAir(){
        player.anim.SetTrigger("FAir");
        attack.MeleeHit(state, player.fAir);
    }
    public override void FSmash(){
        player.anim.SetTrigger("FSmash");
        attack.MeleeHit(state, player.fSmash);
    }
    public override void FDash(){
        player.anim.SetTrigger("FDash");
        attack.MeleeHit(state, player.fDash);
    }
    public override void DTilt(){
        player.anim.SetTrigger("DTilt");
        attack.MeleeHit(state, player.dTilt);
    }
    public override void DNeutral(){
        player.anim.SetTrigger("DNeutral");
        attack.MeleeHit(state, player.dNeutral);
    }
    public override void DAir(){
        player.anim.SetTrigger("DAir");
        attack.MeleeHit(state, player.dAir);
    }
    public override void DSmash(){
        player.anim.SetTrigger("DSmash");
        attack.MeleeHit(state, player.dSmash);
    }

    //Special --------------------------------------------------------

    public override void Special()
    {
        attack.FireProjectile(projectile);
    }
    public override void FSpecial(){

    }
    public override void USpecial()
    {
        
    }
    public override void DSpecial()
    {
        
    }
}
