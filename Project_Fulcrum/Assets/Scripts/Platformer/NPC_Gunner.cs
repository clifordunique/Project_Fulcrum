using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Gunner : NPC 
{

	[SerializeField] public GameObject p_BulletPrefab;

	[SerializeField] public float v_FlashCooldown;
	[SerializeField] public float g_BurstFireDelay;
	[SerializeField] public float g_BurstFireCD;




	//###################################################################################################################################
	// CORE FUNCTIONS
	//###################################################################################################################################
	#region CORE FUNCTIONS



	protected override void FixedUpdateAI()
	{
		FixedUpdateAIThink(); // Gathers information and decides which mode to use

		switch (DecisionState)
		{
			case 0: // Searching
				{
					if(d_aiDebug){print("searching...");}
					o_NavDebugMarker.position=this.transform.position;
					Vector3[] positions = {this.transform.position,o_NavDebugMarker.position};
					o_NavDebugLine.SetPositions(positions);

					NavSearchSimple();

					break;
				}
			case 1: // Pursuing
				{
					if(d_aiDebug){print("Chasing!");}
					NavPursuitRanged();
					break;
				}

			case 2: // Attacking
				{
					if(d_aiDebug){print("ATTACKING!");}
					NavAttackGunSimple();
					break;
				}
			case 3: // Traversing NavMesh
				{
					NavMeshMovement();
					break;
				}
			default: // Idle
				{
					break;
				}
		}
	}
	#endregion	
	protected override void FixedUpdateAIThink() //FUAIT
	{
		if(PunchCooldown > 0)
		{
			PunchCooldown -= Time.fixedDeltaTime;
		}

		if(enemyTarget)
		{
			if(enemyTarget.isAlive())
			{
				goalLocation = enemyTarget.GetPosition()-this.GetPosition();
				distanceToTarget = goalLocation.magnitude;
				if(distanceToTarget <= attackRange)
				{
					DecisionState = 2; // Attacking
				}
				else
				{
					DecisionState = 1; // Pursuing
				}
			}
			else
			{
				enemyTarget = null;
				DecisionState = 0; // Searching
			}
		}
		else
		{
			DecisionState = 0; // Searching
		}
		if(FighterState.Dead)
		{
			DecisionState = -1; // Idle
		}

		if(DecisionState == 1&&!d_DumbNav)
		{
			if((enemyTarget.n_CurrentSurfID != n_CurrentSurfID) && (enemyTarget.n_CurrentSurfID != n_DestSurfID) && (enemyTarget.n_CurrentSurfID!=-1))
			{
				n_DestSurfID = enemyTarget.n_CurrentSurf.id;
			}
			else if(enemyTarget.n_CurrentSurfID == n_CurrentSurfID)
			{
				n_DestSurfID = -1;
			}

			if(n_DestSurfID!=-1) // if a nav destination is set and npc is pursuing, use the nav system.
			{
				DecisionState = 3;
			}
		}

		if(!d_Think)
		{
			DecisionState = 3; // Mindlessly running to a destination which is only set manually.
		}

	}

	protected virtual void FixedUpdateAIAnimation()
	{
		if(PunchDelay<1)
		{
			// change animation
		}
		if(enemyTarget!=null)
		{
			if(enemyTarget.IsDevMode())
			{
				o_NavDebugMarker.gameObject.SetActive(false);
			}
			else
			{
				o_NavDebugMarker.gameObject.SetActive(false);
			}
		}
	}

	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################
	#region CUSTOM FUNCTIONS

	protected void NavPursuitRanged()
	{
		this.FighterState.MouseWorldPos = enemyTarget.GetPosition();

		o_NavDebugMarker.position=(Vector3)this.FighterState.MouseWorldPos;
		Vector3[] positions = {this.transform.position,o_NavDebugMarker.position};
		o_NavDebugLine.SetPositions(positions);

		if(goalLocation.x < 0)
		{
			this.FighterState.LeftKeyHold = true;
			this.FighterState.RightKeyHold = false;
		}
		else if(goalLocation.x > 0)
		{
			this.FighterState.RightKeyHold = true;
			this.FighterState.LeftKeyHold = false;
		}
		else
		{
			this.FighterState.RightKeyHold = false;
			this.FighterState.LeftKeyHold = false;
		}
	}


	protected void FireBullet(Vector2 targetVec)
	{
		Vector2 aimDir = targetVec-(Vector2)this.transform.position;
		float rotation = Get2DAngle(aimDir);
		Quaternion fireAngle = new Quaternion();
		fireAngle.eulerAngles = new Vector3(0, 0, rotation);

		GameObject bulletObj = (GameObject)Instantiate(p_BulletPrefab, this.transform.position+(Vector3)(aimDir.normalized/2), fireAngle, this.transform);
		Bullet b = bulletObj.GetComponent<Bullet>();

		bool[] bulletHitsTeam = new bool[10] {true,true,true,true,true,true,true,true,true,true};
		bulletHitsTeam[this.g_Team] = false;

		b.Fire(aimDir, 200f, 20, this.gameObject, bulletHitsTeam);

		//############################################
		//## Shock Effect Code						##
		//############################################
		GameObject shockEffect = (GameObject)Instantiate(p_ShockEffectPrefab, this.transform.position+(Vector3)(aimDir.normalized/2), fireAngle, this.transform);

		float xTransform = -1f;
		Vector3 theLocalScale = new Vector3 (xTransform, 1f, 1f);

		//shockEffect.GetComponentInChildren<SpriteRenderer>().color = new Color(100,100,100,50);
		shockEffect.transform.localScale = theLocalScale;
		o.fighterAudio.GunshotSound();
	}

	protected void NavAttackGunSimple()
	{
		v_FlashCooldown -= Time.fixedDeltaTime;
		if(PunchCooldown<0.5f&&v_FlashCooldown<=0)
		{
			FlashEffect(0.05f, Color.red);
			v_FlashCooldown = 0.05f;
		}
		else if(PunchCooldown<1&&v_FlashCooldown<=0)
		{
			FlashEffect(0.1f, Color.red);
			v_FlashCooldown = 0.1f;
		}



		o_NavDebugMarker.position=(Vector3)enemyTarget.GetPosition();
		Vector3[] positions = {this.transform.position,o_NavDebugMarker.position};
		o_NavDebugLine.SetPositions(positions);

		this.FighterState.MouseWorldPos = enemyTarget.GetPosition();

		this.FighterState.RightKeyHold = false;
		this.FighterState.LeftKeyHold = false;

		if(PunchCooldown <= 0)
		{
			FireBullet(this.FighterState.MouseWorldPos);
			PunchCooldown = PunchDelay + UnityEngine.Random.Range(-PunchDelayVariance,PunchDelayVariance);
		}
	}





	#endregion
}
