using System.Collections.Generic;
using UnityEngine;

public class PhysicalShockwave : MonoBehaviour
{
	public int damage;

	public float speed;

	public float maxSize;

	public float force;

	public bool hasHurtPlayer;

	public bool enemy;

	public bool noDamageToEnemy;

	private List<Collider> hitColliders = new List<Collider>();

	public EnemyType enemyType;

	public GameObject soundEffect;

	private void Start()
	{
		if (soundEffect != null)
		{
			Object.Instantiate(soundEffect, base.transform.position, Quaternion.identity);
		}
	}

	private void Update()
	{
		base.transform.localScale = new Vector3(base.transform.localScale.x + Time.deltaTime * speed, base.transform.localScale.y, base.transform.localScale.z + Time.deltaTime * speed);
		if (base.transform.localScale.x > maxSize || base.transform.localScale.z > maxSize)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		CheckCollision(collision.collider);
	}

	private void OnTriggerEnter(Collider collision)
	{
		CheckCollision(collision);
	}

	private void CheckCollision(Collider col)
	{
		if (!hasHurtPlayer && col.gameObject.tag == "Player" && col.gameObject.layer != 15)
		{
			hasHurtPlayer = true;
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
			{
				NewMovement instance = MonoSingleton<NewMovement>.Instance;
				instance.GetHurt(damage, invincible: true);
				instance.LaunchFromPoint(instance.transform.position + Vector3.down, 30f, 30f);
			}
			else if (damage == 0)
			{
				MonoSingleton<PlatformerMovement>.Instance.Jump();
			}
			else
			{
				MonoSingleton<PlatformerMovement>.Instance.Explode();
			}
		}
		else
		{
			if (col.gameObject.layer != 10)
			{
				return;
			}
			EnemyIdentifierIdentifier component = col.gameObject.GetComponent<EnemyIdentifierIdentifier>();
			if (!(component != null) || !(component.eid != null) || (enemy && (component.eid.enemyType == enemyType || EnemyIdentifier.CheckHurtException(enemyType, component.eid.enemyType))))
			{
				return;
			}
			Collider component2 = component.eid.GetComponent<Collider>();
			float multiplier = (float)damage / 10f;
			if (noDamageToEnemy || base.transform.localScale.x > 10f || base.transform.localScale.z > 10f)
			{
				multiplier = 0f;
			}
			if (component2 != null && !hitColliders.Contains(component2) && !component.eid.dead)
			{
				hitColliders.Add(component2);
				if (enemy)
				{
					component.eid.hitter = "enemy";
				}
				else
				{
					component.eid.hitter = "explosion";
				}
				if (component.eid.enemyType == EnemyType.Turret && component.eid.TryGetComponent<Turret>(out var component3) && component3.lodged)
				{
					component3.Unlodge();
				}
				component.eid.DeliverDamage(col.gameObject, Vector3.up * force * 2f, col.transform.position, multiplier, tryForExplode: false);
			}
			else if (component2 != null && component.eid.dead)
			{
				hitColliders.Add(component2);
				component.eid.hitter = "explosion";
				component.eid.DeliverDamage(col.gameObject, Vector3.up * 2000f, col.transform.position, multiplier, tryForExplode: false);
			}
		}
	}
}
