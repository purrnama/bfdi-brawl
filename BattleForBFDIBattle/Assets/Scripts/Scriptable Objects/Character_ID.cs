using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "BFBB/Character ID")]
public class Character_ID : ScriptableObject {

	public string characterName = "New Character";

	[Header("Physics")]
	public PhysicMaterial playerPhysic;
	public float mass;
	public float drag;

	[Header("Movement")]
	public float speed = 0f;
	public float jumpPower = 0f;
	public float dashPower = 0f;
	public float dashPeriod = 0f;
	public float divePower = 0f;

	[Header("Damage")]
	public float damage = 0f;

	[Header("Attack Trigger")]
	public Vector3 triggerCenter;
	public Vector3 triggerSize;

	[Header("Animations")]
	public RuntimeAnimatorController animatorController = null;

	[Header("UI")]
	public Sprite icon = null;

	[Header("Tranform follower objects")]
	public Vector3 indicatorOffset = Vector3.zero;
	public Vector3 blockOffset = Vector3.zero;
	public Vector3 blockResize = new Vector3(1f,1f,1f);

	[Header("Audio")]
	public AudioClip[] jumpSounds;
	public AudioClip[] hurtlightSounds;
	public AudioClip[] hurtheavySounds;
	public AudioClip[] attackSounds;
	public AudioClip[] specialSounds;
	public AudioClip[] deathSounds;
	public AudioClip[] blockSounds;

	[Header("-----EXPERIMENTAL-----")]

	[Header("Character Attributes")]

	[Tooltip("The max side speed of a character on the ground.")]
	public float walkSpeed = 0f;

	[Tooltip("How fast a character moves sideways on the ground.")]
	public float walkAcceleration = 0f;

	[Tooltip("How fast a character moves sideways in the air.")]
	public float airAcceleration = 0f;

	[Tooltip("How long it takes for a character to stop moving sideways in the air.")]
	public float airFriction = 0f;

	[Tooltip("The max side speed of a character in the air.")]
	public float airSpeed = 0f;

	[Tooltip("The max jump height for a character.")]
	public float jumpHeight = 0f;

	[Tooltip("How fast a character falls.")]
	public float gravity = 0f;
	
	[Tooltip("How long it takes for a charcter to stop moving sideways on the ground.")]
	public float traction = 0f;
	
	[Tooltip("How much a character can resist knockback.")]
	public float weight = 0f;

	[Tooltip("How much knockback increases as damage increases. This will be divided by 100.")]
	public float knockbackScaling = 0f;

	[Tooltip("How many jumps can this character make while airbourne.")]
	public int maxJumps = 1;

	[Tooltip("How fast can a character jump in a second.")]
	public float jumpRate = 1;
	
	[Tooltip("How fast can a character attack in a second.")]
	public float attackRate = 1;

	[Tooltip("Distance of effective attack from the player. Specify horizontal for sideway attacks and vertical for up-down attacks.")]
	public Vector2 attackDistance = Vector2.zero;

	[Tooltip("The radius of attack range that detects opponents.")]
	public float attackRange = 1;

	[Tooltip("How fast a character dashes on the ground.")]
	public float dashAcceleration = 0f;

	[Tooltip("Does jumping towards opposite direction snaps the velocity to that direction in the air?")]
	public bool isJumpingSnapped = false;

	[Header("Attack Attributes")]
	
	[Tooltip("Damage value for neutral ground attack.")]
	public float jabDamage = 0f;
	[Range(0.0f, 2.0f)]
	public float jabAngle = 0f;

	[Space]
	
	[Tooltip("Damage value for neutral aerial attack.")]
	public float nAirDamage = 0f;
	[Range(0.0f, 2.0f)]
	public float nAirAngle = 0f;
	
	[Space]
	
	[Tooltip("Damage value for forward ground attack.")]
	public float fSmashDamage = 0f;
	[Range(0.0f, 2.0f)]
	public float fSmashAngle = 0f;
	
	[Space]
	
	[Tooltip("Damage value for forward ground attack.")]
	public float fDashDamage = 0f;
	[Range(0.0f, 2.0f)]
	public float fDashAngle = 0f;
	
	[Space]

	[Tooltip("Damage value for forward aerial attack.")]
	public float fAirDamage = 0f;
	[Range(0.0f, 2.0f)]
	public float fAirAngle = 0f;

	[Space]

	[Tooltip("Damage value for up grounded attack.")]
	public float uSmashDamage = 0f;
	[Range(0.0f, 2.0f)]
	public float uSmashAngle = 0f;

	[Space]

	[Tooltip("Damage value for up aerial attack.")]
	public float uAirDamage = 0f;
	[Range(0.0f, 2.0f)]
	public float uAirAngle = 0f;

	[Space]

	[Tooltip("Damage value for down grounded attack.")]
	public float dSmashDamage = 0f;
	[Range(0.0f, 2.0f)]
	public float dSmashAngle = 0f;

	[Space]

	[Tooltip("Damage value for down aerial attack.")]
	public float dAirDamage = 0f;
	[Range(0.0f, 2.0f)]
	public float dAirAngle = 0f;

}
