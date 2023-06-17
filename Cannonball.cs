using System.Collections.Generic;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
	public bool launchable = true;

	[SerializeField]
	public bool launched;

	private Rigidbody rb;

	private Collider col;

	[SerializeField]
	private GameObject breakEffect;

	private bool checkingForBreak;

	private bool broken;

	public float damage;

	public float speed;

	public bool parry;

	[HideInInspector]
	public Sisyphus sisy;

	public bool ghostCollider;

	public bool canBreakBeforeLaunched;

	[Header("Physics Cannonball Settings")]
	public bool physicsCannonball;

	public AudioSource bounceSound;

	[HideInInspector]
	public List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

	public int maxBounces;

	private int currentBounces;

	[HideInInspector]
	public bool hasBounced;

	[HideInInspector]
	public bool forceMaxSpeed;

	public int durability = 99;

	[SerializeField]
	private GameObject interruptionExplosion;

	[SerializeField]
	private GameObject groundHitShockwave;

	[HideInInspector]
	public GameObject sourceWeapon;

	private TimeSince instaBreakDefence;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		col = GetComponent<Collider>();
		instaBreakDefence = 1f;
		if (physicsCannonball)
		{
			MonoSingleton<GrenadeList>.Instance.AddCannonball(this);
		}
	}

	private void OnDestroy()
	{
		if (physicsCannonball)
		{
			MonoSingleton<GrenadeList>.Instance.RemoveCannonball(this);
		}
	}

	private void FixedUpdate()
	{
		if (launched)
		{
			rb.velocity = base.transform.forward * speed;
		}
		if (physicsCannonball && (bool)groundHitShockwave && rb.velocity.magnitude > 0f && rb.SweepTest(rb.velocity.normalized, out var hitInfo, rb.velocity.magnitude * Time.fixedDeltaTime) && (hitInfo.transform.gameObject.layer == 8 || hitInfo.transform.gameObject.layer == 24) && Vector3.Angle(hitInfo.normal, Vector3.up) < 45f)
		{
			GameObject obj = Object.Instantiate(groundHitShockwave, hitInfo.point + hitInfo.normal * 0.1f, Quaternion.identity);
			obj.transform.up = hitInfo.normal;
			if (obj.TryGetComponent<PhysicalShockwave>(out var component))
			{
				component.force = 10000f + rb.velocity.magnitude * 80f;
			}
			Break();
		}
		if (hitEnemies.Count <= 0)
		{
			return;
		}
		for (int num = hitEnemies.Count - 1; num >= 0; num--)
		{
			if (hitEnemies[num] == null || Vector3.Distance(base.transform.position, hitEnemies[num].transform.position) > 20f)
			{
				hitEnemies.RemoveAt(num);
			}
		}
	}

	public void Launch()
	{
		if (launchable)
		{
			launched = true;
			rb.isKinematic = false;
			rb.useGravity = false;
			col.isTrigger = true;
			hitEnemies.Clear();
			InstaBreakDefenceCancel();
			if (currentBounces == 1 && hasBounced)
			{
				damage += 2f;
			}
			currentBounces++;
			if ((bool)sisy)
			{
				sisy.GotParried();
			}
		}
	}

	public void Unlaunch(bool relaunchable = true)
	{
		launchable = relaunchable;
		launched = false;
		if ((bool)rb)
		{
			rb.isKinematic = !physicsCannonball;
			rb.useGravity = physicsCannonball;
			rb.velocity = Vector3.zero;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!ghostCollider || (launched && (other.gameObject.layer == 10 || other.gameObject.layer == 11 || other.gameObject.layer == 12)))
		{
			Collide(other);
		}
	}

	public void Collide(Collider other)
	{
		if ((launched || canBreakBeforeLaunched) && !other.isTrigger && (other.gameObject.layer == 8 || other.gameObject.layer == 24 || (launched && other.gameObject.layer == 0 && (other.gameObject.tag != "Player" || !col.isTrigger))))
		{
			Break();
		}
		else
		{
			if ((!launched && !physicsCannonball) || (other.gameObject.layer != 10 && other.gameObject.layer != 11 && other.gameObject.layer != 12) || checkingForBreak)
			{
				return;
			}
			checkingForBreak = true;
			EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
			EnemyIdentifier enemyIdentifier = ((!component || !component.eid) ? other.gameObject.GetComponent<EnemyIdentifier>() : component.eid);
			if ((bool)enemyIdentifier && !hitEnemies.Contains(enemyIdentifier))
			{
				if (physicsCannonball && (float)instaBreakDefence < 1f)
				{
					hitEnemies.Add(enemyIdentifier);
					return;
				}
				bool flag = true;
				if (!enemyIdentifier.dead)
				{
					flag = false;
				}
				enemyIdentifier.hitter = "cannonball";
				if (!physicsCannonball)
				{
					enemyIdentifier.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 100f, base.transform.position, damage, tryForExplode: true);
				}
				else if (forceMaxSpeed)
				{
					enemyIdentifier.DeliverDamage(other.gameObject, base.transform.forward.normalized * 1000f, base.transform.position, damage, tryForExplode: true);
				}
				else if (rb.velocity.magnitude > 10f)
				{
					enemyIdentifier.DeliverDamage(other.gameObject, rb.velocity.normalized * rb.velocity.magnitude * 1000f, base.transform.position, Mathf.Min(damage, rb.velocity.magnitude * 0.15f), tryForExplode: true);
				}
				hitEnemies.Add(enemyIdentifier);
				if (!enemyIdentifier || enemyIdentifier.dead)
				{
					if (!flag)
					{
						MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.cannonballed", sourceWeapon, enemyIdentifier);
						durability--;
						if (durability <= 0)
						{
							Break();
						}
					}
					if (physicsCannonball && !launched && (!flag || other.gameObject.layer == 11))
					{
						Bounce();
					}
					if ((bool)enemyIdentifier)
					{
						enemyIdentifier.Explode();
					}
					checkingForBreak = false;
				}
				else
				{
					if (!physicsCannonball || rb.velocity.magnitude < 15f)
					{
						Break();
					}
					else
					{
						Bounce();
					}
					if (enemyIdentifier.enemyType == EnemyType.Sisyphus && enemyIdentifier.TryGetComponent<Sisyphus>(out var component2))
					{
						component2.Knockdown(base.transform.position);
					}
				}
			}
			else
			{
				checkingForBreak = false;
			}
		}
	}

	public void Break()
	{
		if ((bool)sisy)
		{
			checkingForBreak = false;
			launched = false;
			launchable = false;
			rb.useGravity = true;
			rb.velocity = Vector3.up * 25f;
			MonoSingleton<CameraController>.Instance.CameraShake(1f);
			if ((bool)breakEffect)
			{
				Object.Instantiate(breakEffect, base.transform.position, base.transform.rotation);
			}
			sisy.SwingStop();
		}
		else if (!broken)
		{
			broken = true;
			if ((bool)breakEffect)
			{
				Object.Instantiate(breakEffect, base.transform.position, base.transform.rotation);
			}
			Object.Destroy(base.gameObject);
		}
	}

	private void Bounce()
	{
		if (currentBounces >= maxBounces)
		{
			Break();
			return;
		}
		instaBreakDefence = 0f;
		currentBounces++;
		durability = 99;
		hasBounced = true;
		launched = false;
		launchable = true;
		checkingForBreak = false;
		rb.useGravity = true;
		rb.velocity = Vector3.up * rb.velocity.magnitude * 0.15f + rb.velocity.normalized * -5f;
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
		if ((bool)bounceSound)
		{
			Object.Instantiate(bounceSound, base.transform.position, Quaternion.identity);
		}
	}

	public void Explode()
	{
		if ((bool)interruptionExplosion)
		{
			Object.Instantiate(interruptionExplosion, base.transform.position, Quaternion.identity);
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleExplosions"))
		{
			breakEffect = null;
		}
		Break();
	}

	public void InstaBreakDefenceCancel()
	{
		instaBreakDefence = 1f;
	}
}
