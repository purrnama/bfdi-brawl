using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "BFBB/Character ID")]
public class Character_ID : ScriptableObject {

	public string characterName = "New Character";

	[Header("Character Controller Attributes")]
	public Vector3 center;
	public float radius;
	public float height;

	[Header("Smash Character Attributes")]

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

	[Tooltip("Fast falling speed increase in percentage.")]
	[Range(0.00f, 100.00f)]
	public float fastFallingPercentage =  0.00f;

	public int spotDodgeFrames = 0;

	public float shieldMaxSize = 0;
	
	public float shieldMinSize = 0;

	public float grabRange = 0;

	[Header("Attack Attributes")]
	[Space]
	public Vector3 uNeutral = Vector3.zero;
	public Vector3 uAir = Vector3.zero;
	public Vector3 uSmash = Vector3.zero;
	public Vector3 uTilt = Vector3.zero;
	public Vector3 jab = Vector3.zero;
	public Vector3 fNeutral = Vector3.zero;
	public Vector3 fAir = Vector3.zero;
	public Vector3 fSmash = Vector3.zero;
	public Vector3 fDash = Vector3.zero;
	public Vector3 dTilt = Vector3.zero;
	public Vector3 dNeutral = Vector3.zero;
	public Vector3 dAir = Vector3.zero;
	public Vector3 dSmash = Vector3.zero;

	public List<GameObject> CharacterAssets;
}
