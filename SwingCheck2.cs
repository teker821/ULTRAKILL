using System.Collections.Generic;
using UnityEngine;

public class SwingCheck2 : MonoBehaviour
{
	[HideInInspector]
	public EnemyIdentifier eid;

	public EnemyType type;

	public bool playerOnly;

	public bool playerBeenHit;

	private NewMovement nmov;

	public int damage;

	public int enemyDamage;

	public float knockBackForce;

	public bool knockBackDirectionOverride;

	public Vector3 knockBackDirection;

	private LayerMask lmask;

	private List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

	public bool strong;

	[HideInInspector]
	public Collider col;

	public Collider[] additionalColliders;

	public bool useRaycastCheck;

	private AudioSource aud;

	private bool physicalCollider;

	[HideInInspector]
	public bool damaging;

	public bool ignoreSlidingPlayer;

	public bool startActive;

	public bool interpolateBetweenFrames;

	private Vector3 previousPosition;

	private bool ignoreTick;

	private void Start()
	{
		if (!eid)
		{
			eid = GetComponentInParent<EnemyIdentifier>();
		}
		if ((bool)eid)
		{
			type = eid.enemyType;
		}
		col = GetComponent<Collider>();
		if (!col.isTrigger)
		{
			physicalCollider = true;
		}
		else if (!startActive)
		{
			col.enabled = false;
		}
		else
		{
			DamageStart();
		}
		if (interpolateBetweenFrames)
		{
			previousPosition = base.transform.position;
		}
		aud = GetComponent<AudioSource>();
		lmask = (int)lmask | 0x100;
		lmask = (int)lmask | 0x1000000;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (damaging)
		{
			CheckCollision(other);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (damaging)
		{
			CheckCollision(collision.collider);
		}
	}

	private void Update()
	{
		if (!interpolateBetweenFrames || !damaging || !col.attachedRigidbody)
		{
			return;
		}
		if (!ignoreTick)
		{
			RaycastHit[] array = col.attachedRigidbody.SweepTestAll(previousPosition - base.transform.position, Vector3.Distance(previousPosition, base.transform.position), QueryTriggerInteraction.Collide);
			foreach (RaycastHit raycastHit in array)
			{
				CheckCollision(raycastHit.collider);
			}
		}
		else
		{
			ignoreTick = false;
		}
		previousPosition = base.transform.position;
	}

	private void CheckCollision(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			if (playerBeenHit || other.gameObject.layer == 15)
			{
				return;
			}
			bool flag = false;
			if (useRaycastCheck && (bool)eid)
			{
				Vector3 vector = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
				if (Physics.Raycast(vector, other.bounds.center - vector, Vector3.Distance(vector, other.bounds.center), lmask))
				{
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				if (!ignoreSlidingPlayer || !MonoSingleton<PlatformerMovement>.Instance.sliding)
				{
					MonoSingleton<PlatformerMovement>.Instance.Explode();
				}
				return;
			}
			if (nmov == null)
			{
				nmov = other.GetComponent<NewMovement>();
			}
			if (ignoreSlidingPlayer && nmov.sliding)
			{
				return;
			}
			nmov.GetHurt(Mathf.RoundToInt((float)damage * eid.totalDamageModifier), invincible: true);
			playerBeenHit = true;
			if (knockBackForce > 0f)
			{
				Vector3 forward = base.transform.forward;
				if (knockBackDirectionOverride)
				{
					forward = knockBackDirection;
				}
				if (knockBackDirection == Vector3.down)
				{
					nmov.Slamdown(knockBackForce);
				}
				else
				{
					nmov.LaunchFromPoint(nmov.transform.position + forward * -1f, knockBackForce, knockBackForce);
				}
			}
			if ((bool)eid)
			{
				eid.SendMessage("PlayerBeenHit", SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (other.gameObject.layer == 10 && !playerOnly)
		{
			EnemyIdentifierIdentifier component = other.GetComponent<EnemyIdentifierIdentifier>();
			if (!(component != null) || !(component.eid != null) || component.eid.enemyType == type || EnemyIdentifier.CheckHurtException(type, component.eid.enemyType))
			{
				return;
			}
			EnemyIdentifier enemyIdentifier = component.eid;
			if ((hitEnemies.Contains(enemyIdentifier) && (!enemyIdentifier.dead || !(other.gameObject.tag == "Head"))) || (enemyIdentifier.dead && (!enemyIdentifier.dead || !(other.gameObject.tag != "Body"))))
			{
				return;
			}
			bool flag2 = false;
			if (useRaycastCheck && (bool)eid)
			{
				Vector3 vector2 = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
				if (Physics.Raycast(vector2, other.transform.position - vector2, Vector3.Distance(vector2, other.transform.position), lmask))
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				enemyIdentifier.hitter = "enemy";
				if (enemyDamage == 0)
				{
					enemyDamage = damage;
				}
				enemyIdentifier.DeliverDamage(other.gameObject, ((base.transform.position - other.transform.position).normalized + Vector3.up) * 10000f, other.transform.position, (float)(enemyDamage / 10) * eid.totalDamageModifier, tryForExplode: false);
				hitEnemies.Add(enemyIdentifier);
			}
		}
		else if (other.gameObject.tag == "Breakable")
		{
			Breakable component2 = other.gameObject.GetComponent<Breakable>();
			if (component2 != null && (strong || component2.weak) && !component2.playerOnly && !component2.precisionOnly)
			{
				component2.Break();
			}
		}
	}

	public void DamageStart()
	{
		ignoreTick = true;
		damaging = true;
		if (!physicalCollider)
		{
			col.enabled = true;
			if (additionalColliders != null)
			{
				Collider[] array = additionalColliders;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = true;
				}
			}
		}
		if (aud != null)
		{
			aud.Play();
		}
	}

	public void DamageStop()
	{
		damaging = false;
		playerBeenHit = false;
		if (hitEnemies.Count > 0)
		{
			hitEnemies.Clear();
		}
		if (physicalCollider)
		{
			return;
		}
		if ((bool)col)
		{
			col.enabled = false;
		}
		if (additionalColliders != null)
		{
			Collider[] array = additionalColliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
	}

	public void OverrideEnemyIdentifier(EnemyIdentifier newEid)
	{
		eid = newEid;
	}
}
