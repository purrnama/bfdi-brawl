using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveset : MonoBehaviour
{
    internal Attack_Controller attack;
    internal PlayerMovement player;
    internal Character_ID id;
    internal Vector2 state;

    //Normal--------------------------------------------
    public virtual void UNeutral(){
    }
    public virtual void UAir(){
    }
    public virtual void USmash(){
    }
    public virtual void UTilt(){
    }
    public virtual void Jab(){
    }
    public virtual void FNeutral(){
    }
    public virtual void FAir(){
    }
    public virtual void FSmash(){
    }
    public virtual void FDash(){
    }
    public virtual void DTilt(){
    }
    public virtual void DNeutral(){
    }
    public virtual void DAir(){
    }
    public virtual void DSmash(){
    }
    

    //Special-----------------------------------------------
    public virtual void Special(){
    }
    public virtual void FSpecial(){
    }
    public virtual void USpecial(){
    }
    public virtual void DSpecial(){
    }
}
