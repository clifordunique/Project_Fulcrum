using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	[SerializeField] protected LayerMask m_CollisionMask;	// Mask used for bullet collisions.
	[SerializeField] protected Vector2 vel;
	[SerializeField] protected int damage;
	[SerializeField] protected GameObject source; // Used to exclude the shooter from collision.
	[SerializeField][ReadOnlyAttribute] protected bool[] collidesWithTeam;
//	[SerializeField] protected Quaternion myRotation; 
//	[SerializeField] protected Vector2 pos;

	void FixedUpdate()
	{
		float crntSpeed = vel.magnitude*Time.fixedDeltaTime; //Current speed.
		RaycastHit2D[] bulletCollision = Physics2D.RaycastAll(this.transform.position, vel, crntSpeed, m_CollisionMask);

		foreach(RaycastHit2D hit in bulletCollision)
		{
			if((hit.collider.gameObject != this.gameObject) && (hit.collider.gameObject != source))
			{
				if(hit.collider.GetComponent<FighterChar>())
				{
					FighterChar opponent = hit.collider.GetComponent<FighterChar>();
					if(opponent.isAlive() && collidesWithTeam[opponent.GetTeam()])
					{
						opponent.TakeDamage(damage);
						opponent.v.triggerFlinched = true;
						bool facingdir = true;
						if((opponent.GetPosition().x-this.transform.position.x)>0)
						{
							facingdir = false;
						}
						if(opponent.IsPlayer())
						{
							// Cause screen shake.
						}

						hit.collider.GetComponent<FighterChar>().v.facingDirection = facingdir;
						Destroy(this.transform.gameObject); // Only destroy if hitting an alive, enemy fighter.
					}
				}
				else // Hit inanimate object
				{
					Destroy(this.transform.gameObject);
				}
			}
		}

		this.transform.position += (Vector3)vel*Time.fixedDeltaTime;
//		this.transform.rotation = myRotation; 
	}

	public void Fire(Vector2 direction, float speed, int dmg, GameObject src, bool[] teamCollisions)
	{
		damage = dmg;
		vel = direction.normalized*speed;
		source = src;
		collidesWithTeam = teamCollisions;
	}

	public void Fire(Vector2 direction, float speed, int dmg, GameObject src)
	{
		damage = dmg;
		vel = direction.normalized*speed;
		source = src;
		collidesWithTeam = new bool[] {true,true,true,true,true,true,true,true,true,true};
	}




}
