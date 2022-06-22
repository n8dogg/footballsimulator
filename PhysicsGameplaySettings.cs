using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.XR;
using UnityEngine.Experimental.XR;
//using Unity.XR.Oculus;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class PhysicsGameplaySettings : GameDataFile
{
	//File name: physics_gameplay_settings.json

	//Defaults, for reference. Don't change these.
	[SerializeField] [Range(1.0f, 500.0f)] public float muscleSpring = 120f;					//The spring force of the muscles themselves. Higher = stronger muscles
	[SerializeField] [Range(0.0f, 500.0f)] public float muscleDamper = 5f;						//Damping of the muscle force. Lower has sharper animations. Higher for animations that glitch less.
	[SerializeField] [Range(1.0f, 5.0f)] public float pinPow = 1f;								//The power of pinning the puppet to the character. Higher for shaper animations.
	[SerializeField] [Range(1.0f, 150.0f)] public float pinDistanceFalloff = 55f;				//How much pin power degrades as you get farther from the target. Lower for tighter gameplay. Higher for looser.

	[SerializeField] [Range(0.0f, 2.5f)] public float AdditionalForceOnCollision_coeff = 0f;	//Extra force on hit. Turn up to make hitting more powerful. Has side effects like launching the ball too far.
	[SerializeField] [Range(0.05f, 0.5f)] public float SpeedBoostResetModifier_coeff = 0.25f;	//How fast you get back to normal speed after a speed reduction. Lower for slower regain. Higher for more instant regain speed.

	[SerializeField] [Range(0.0f, 1.0f)] public float originalPinWeight = 1f;					//The max root pin weight coefficient. Lower than 1 means a more wobbly character.
	[SerializeField] [Range(0.0f, 1.0f)] public float originalMusclePinWeight = 1f;				//The max muscle pin weight coefficient. Just operates on the muscles not the root/hips.

	[SerializeField] public float AI_CoverageAgility_coeff =  0.00175f;							//Coefficient for moving AI characters around. Turn this up to increase the game's difficulty.
	[SerializeField] public float Player_CoverageAgility_coeff = 0.0015f;						//Coefficient for moving Player characters around. Turn this up to decrease the game's difficulty.
	[SerializeField] public float StoppingAgility_coeff = 6.25f;								//How hard people can stop. Usually 1. Turn up to allow harder stopping or turn down for more sliding.

	[SerializeField] public float swingAmount = 1f;												//Acceleration when turning amount. Usually 1 to 2.5.
	[SerializeField] public float SwingVelocityCurve_exp = 0.79f;								//Curve used for acceleration and acceleration turning. Usually 0.79 to 0.85

	[SerializeField] public float ThrowAngle_Max = 40f;											//The maximum angle the QB can throw the ball (up in the air)
	[SerializeField] public float ThrowAngle_Min = 12.5f;										//The minimum angle the QB can throw the ball (bullet passing)

	[SerializeField] public float regainPinSpeed = 0.25f;										//How fast the puppet recovers from being knocked out of alignment. Higher = harder to tackle people
	[SerializeField] public float CollisionResistance_coeff = 35f;								//Higher means more collision restance, so more resistance to hits and harder to fall down.

	[SerializeField] public bool useBehaviorPuppetOverrides = true;								//Whether or not to use behavior puppet overrides, which is hard coded to allow the hips and chest to use different values.
	[SerializeField] public float override_hipsRegainPinSpeed_coeff = 0.35f;					//Hips recovery coefficient
	[SerializeField] public float override_chestRegainPinSpeed_coeff = 8f;						//Chest receovery coefficient, which we make large to allow for dragging defenders.

	[SerializeField] public float knockOutDistanceModifier = 3f;								//How far, in meters, a bone can be before the character looses balance and falls down.
	[SerializeField] public float collisionResistanceModifier = 0.5f;							//Coefficient for the collision resistance. Why we have two, I have no idea.
	[SerializeField] public float collisionThreshold = 0.1f;									//Minimum distance threshold for a collision to occur. Usually 0.1 but 0.01 can lead to interesting results.

	[SerializeField] public float dive_BoostImmunity = 25f;										//Puppetmaster muscle immunity when you dive, so you go through people. Turn up to make tackling more powerful.
	[SerializeField] public float dive_BoostImpulseMlp = 125f;									//The physical impulse you apply to things you hit. Turn it up to make tackling and hitting harder.

	[SerializeField] public float stiffArm_pinCoeff = 1.5f;										//Extra pin weight for the lower and upper arm doing the stiff arm.
	[SerializeField] public float stiffArm_BoostImmunity = 10f;									//Immunity for the arms
	[SerializeField] public float stiffArm_BoostImpulseMlp = 50f;								//Impulse power for the arms
	[SerializeField] public float stiffArm_SpeedBoostModifier_coeff = 0.95f;					//Speed boost (or slowdown) coefficient during stiff arm

	[SerializeField] public float powerRush_pinCoeff = 1.5f;									//Extra pin weight for the shoulders doing the power rush.
	[SerializeField] public float powerRush_BoostImmunity = 10f;								//Immunity for the shoulders
	[SerializeField] public float powerRush_BoostImpulseMlp = 50f;								//Impulse power for the shoulders
	[SerializeField] public float powerRush_SpeedBoostModifier_coeff = 0.95f;					//Speed boost (or slowdown) coefficient during power rush

	[SerializeField] public float grabTackle_slowDown_coeff = 0.935f;	//0.915					//Slowdown when being grabbed coefficient
	[SerializeField] public float grabTackle_unPinAll_coeff = 0.96f;	//0.97					//Unpin muscles when grabbed
	[SerializeField] public float grabTackle_musclePullDownForce_coeff = 0.5f;					//Muscle pull down force when grabbing
	[SerializeField] public float grabTackle_handMuscleUnPin_coeff = 0.865f;					//Unpin coefficient of the hand that grabs
	[SerializeField] public float grabTackle_grabbedMuscleUnPin_coeff = 0.85f;					//Unpin coefficient of the muscle being grabbed
	[SerializeField] public float grabTackle_grabbedMuscleParentUnPin_coeff = 0.85f;			//Unpin coefficient of the parent of the muscle being grabbed
	[SerializeField] public float grabTackle_muscleGrabDistance = 0.5f;							//How far away your hand can be until it lets go of the grab. Lower to make tackle breaking easier.
	[SerializeField] public float grabTackle_muscleDamper_coeff = 1.01f;						//Damping of muscles being grabbed
	[SerializeField] public float grabTackle_handGrabVelocity_coeff = 35f;						//Velocity coefficient for pulling the grabbed muscle
	[SerializeField] public float grabTackle_handParentGrabVelocity_coeff = 25f;				//Velocity coefficient for pulling the parent of the grabbed muscle

	[SerializeField] public float dive_diveForceTime = 1f;										//How much time in seconds force is applied during a dive
	[SerializeField] public float dive_animationCrossfadeTime = 0.35f;							//How long the animation cross fade takes to the Dive animation
	[SerializeField] public float dive_upPower = 600f;											//Up power force component of the dive vector
	[SerializeField] public float dive_upPower_withBallCoeff = 1.1f;							//Extra jump up power when you have teh ball
	[SerializeField] public float dive_forwardPowerDistanceToTarget_coeff = 3750f;	//1250		//How much forward power diving applies
	[SerializeField] public float dive_AITackleDistanceMin = 1.5f;								//Lowest possible AI distance for diving.
	[SerializeField] public float dive_AITackleDistanceMax = 4.75f;								//Highest possible AI distance for diving.
	[SerializeField] public float dive_AITackleDistance_coeff = 0.065f;							//Multiplied by the player's square magnitude to get the dive distance, which is clamped to the min and max.

	[SerializeField] public float muscleCollision_knockHelmetOffForce = 500f;					//How much force required to knock a helmet off.
	[SerializeField] public float slerpCapsule_coeff = 3f;										//Slerp for rotating the capsule back to standing. Multiplied by Time.fixedDeltaTime in code.
	[SerializeField] public float cutSpeedBoostModified_coeff = 0.75f;							//Speed boost modified when doing cut animation
	[SerializeField] public float characterRotationSpeed_coeff = 1f;							//How fast the character capsule rotates

	[SerializeField] public float fumbleCollisionDistanceMax = 0.75f;							//The maximum distance between objects colliding to cause a fumble
	[SerializeField] public float fumbleForceMultiplier = 3f;									//Multiplier for forces applied that can cause fumbles. Increase to increase fumble frequency.
	[SerializeField] public float fumbleThresholdPassOrCatch_coeff = 0.5f;						//Multiplier to make it easier to knock the ball loose while its in the process of being caught
	[SerializeField] public float passBreakup_AttackAngleDiff = 25f;							//At what angle their velocity must match where the ball is to break it up
	[SerializeField] public float passBreakup_distanceToBall = 2.5f;							//How far away a defender will lunge after a pass is caught
	[SerializeField] public float dropBallThreshold = 0.6f;										//How much time must pass before a catch is officially *caught*. Before that a fumble is an incomplete pass.
	[SerializeField] public float throwLead_coeff = 1.05f;										//How far QBs lead receivers on throws.

	[SerializeField] [Range(0.05f, 1f)] public float playerCapsuleRadius = 0.425f;				//How large the player capsule is.
}