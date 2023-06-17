using System;
using System.Collections.Generic;
using UnityEngine;

public class Nail : MonoBehaviour
{
	public GameObject sourceWeapon;

	[HideInInspector]
	public bool hit;

	public float damage;

	private AudioSource aud;

	[HideInInspector]
	public Rigidbody rb;

	public AudioClip environmentHitSound;

	public AudioClip enemyHitSound;

	public Material zapMaterial;

	public GameObject zapParticle;

	private bool zapped;

	public bool fodderDamageBoost;

	public string weaponType;

	public bool heated;

	[HideInInspector]
	public List<Magnet> magnets = new List<Magnet>();

	private bool launched;

	[HideInInspector]
	public NailBurstController nbc;

	public bool enemy;

	public EnemyType safeEnemyType;

	private Vector3 startPosition;

	[Header("Sawblades")]
	public bool sawblade;

	public float hitAmount = 3.9f;

	private EnemyIdentifier currentHitEnemy;

	private float sameEnemyHitCooldown;

	[SerializeField]
	private GameObject sawBreakEffect;

	[SerializeField]
	private GameObject sawBounceEffect;

	[HideInInspector]
	public int magnetRotationDirection;

	private List<Transform> hitLimbs = new List<Transform>();

	private float removeTimeMultiplier = 1f;

	public bool bounceToSurfaceNormal;

	[HideInInspector]
	public bool stopped;

	public int multiHitAmount = 1;

	private int currentMultiHitAmount;

	private float multiHitCooldown;

	private Transform hitTarget;

	[HideInInspector]
	public Vector3 originalVelocity;

	public AudioSource stoppedAud;

	[HideInInspector]
	public bool punchable;

	[HideInInspector]
	public bool punched;

	[HideInInspector]
	public float punchDistance;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		if (!rb)
		{
			rb = GetComponent<Rigidbody>();
		}
		if (sawblade)
		{
			removeTimeMultiplier = 3f;
		}
		if (magnets.Count == 0)
		{
			Invoke("RemoveTime", 5f * removeTimeMultiplier);
		}
		Invoke("MasterRemoveTime", 60f);
		startPosition = base.transform.position;
		Invoke("SlowUpdate", 2f);
	}

	private void OnDestroy()
	{
		if (zapped)
		{
			UnityEngine.Object.Instantiate(zapParticle, base.transform.position, base.transform.rotation);
		}
	}

	private void SlowUpdate()
	{
		if (Vector3.Distance(base.transform.position, startPosition) > 1000f)
		{
			RemoveTime();
		}
		else
		{
			Invoke("SlowUpdate", 2f);
		}
	}

	private void Update()
	{
		if (!hit)
		{
			if (!rb)
			{
				rb = GetComponent<Rigidbody>();
			}
			if ((bool)rb)
			{
				base.transform.LookAt(base.transform.position + rb.velocity * -1f);
			}
		}
		if (sameEnemyHitCooldown > 0f && !stopped)
		{
			sameEnemyHitCooldown = Mathf.MoveTowards(sameEnemyHitCooldown, 0f, Time.deltaTime);
			if (sameEnemyHitCooldown <= 0f)
			{
				currentHitEnemy = null;
			}
		}
		if (multiHitAmount <= 1)
		{
			return;
		}
		if (multiHitCooldown > 0f)
		{
			multiHitCooldown = Mathf.MoveTowards(multiHitCooldown, 0f, Time.deltaTime);
		}
		else if (stopped)
		{
			if (!currentHitEnemy.dead && currentMultiHitAmount > 0)
			{
				currentMultiHitAmount--;
				hitAmount -= 1f;
				DamageEnemy(hitTarget, currentHitEnemy);
			}
			if (currentHitEnemy.dead || currentMultiHitAmount <= 0)
			{
				stopped = false;
				rb.velocity = originalVelocity;
				if (hitAmount <= 0f)
				{
					SawBreak();
				}
				return;
			}
			multiHitCooldown = 0.15f;
		}
		if ((bool)stoppedAud)
		{
			if (stopped)
			{
				stoppedAud.pitch = 2f;
				stoppedAud.volume = 0.5f;
			}
			else
			{
				stoppedAud.pitch = 1f;
				stoppedAud.volume = 0.25f;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!sawblade || !rb || hit)
		{
			return;
		}
		if (stopped)
		{
			rb.velocity = Vector3.zero;
			return;
		}
		if (magnets.Count > 0)
		{
			for (int num = magnets.Count - 1; num >= 0; num--)
			{
				if (magnets[num] == null)
				{
					magnets.RemoveAt(num);
				}
			}
			if (magnets.Count == 0)
			{
				return;
			}
			Magnet targetMagnet = GetTargetMagnet();
			if (!targetMagnet)
			{
				return;
			}
			if (punched)
			{
				if (Vector3.Distance(base.transform.position, targetMagnet.transform.position) > punchDistance)
				{
					punched = false;
					punchDistance = 0f;
					rb.velocity = Vector3.RotateTowards(rb.velocity, Quaternion.Euler(0f, 85 * magnetRotationDirection, 0f) * (targetMagnet.transform.position - base.transform.position).normalized * rb.velocity.magnitude, float.PositiveInfinity, rb.velocity.magnitude);
				}
			}
			else
			{
				rb.velocity = Vector3.RotateTowards(rb.velocity, Quaternion.Euler(0f, 85 * magnetRotationDirection, 0f) * (targetMagnet.transform.position - base.transform.position).normalized * rb.velocity.magnitude, float.PositiveInfinity, rb.velocity.magnitude);
			}
		}
		RaycastHit[] array = rb.SweepTestAll(rb.velocity.normalized, rb.velocity.magnitude * Time.fixedDeltaTime, QueryTriggerInteraction.Ignore);
		if (array == null || array.Length == 0)
		{
			return;
		}
		Array.Sort(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = array[i].transform.gameObject;
			if (!hit && (gameObject.layer == 10 || gameObject.layer == 11) && (gameObject.tag == "Head" || gameObject.tag == "Body" || gameObject.tag == "Limb" || gameObject.tag == "EndLimb" || gameObject.tag == "Enemy"))
			{
				TouchEnemy(gameObject.transform);
			}
			else
			{
				if (gameObject.layer != 8 && gameObject.layer != 24 && gameObject.layer != 26 && !(gameObject.tag == "Armor"))
				{
					continue;
				}
				if (gameObject.TryGetComponent<Breakable>(out var component) && component.weak)
				{
					component.Break();
					return;
				}
				if (hitAmount <= 0f)
				{
					SawBreak();
					return;
				}
				base.transform.position = array[i].point;
				if (bounceToSurfaceNormal)
				{
					rb.velocity = array[i].normal * rb.velocity.magnitude;
				}
				else
				{
					rb.velocity = Vector3.Reflect(rb.velocity.normalized, array[i].normal) * rb.velocity.magnitude;
				}
				flag = true;
				GameObject gameObject2 = UnityEngine.Object.Instantiate(sawBounceEffect, array[i].point, Quaternion.LookRotation(array[i].normal));
				if (flag2 && gameObject2.TryGetComponent<AudioSource>(out var component2))
				{
					component2.enabled = false;
				}
				else
				{
					flag2 = true;
				}
				punched = false;
				punchable = true;
				if (magnets.Count > 0)
				{
					magnetRotationDirection *= -1;
					hitAmount -= 0.1f;
				}
				else
				{
					hitAmount -= 0.25f;
				}
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < 3; j++)
		{
			if (!Physics.Raycast(base.transform.position, rb.velocity.normalized, out var hitInfo, 5f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				break;
			}
			if (hitInfo.transform.TryGetComponent<Breakable>(out var component3) && component3.weak)
			{
				component3.Break();
				break;
			}
			base.transform.position = hitInfo.point;
			if (bounceToSurfaceNormal)
			{
				rb.velocity = hitInfo.normal * rb.velocity.magnitude;
			}
			else
			{
				rb.velocity = Vector3.Reflect(rb.velocity.normalized, hitInfo.normal) * rb.velocity.magnitude;
			}
			hitAmount -= 0.125f;
			GameObject gameObject3 = UnityEngine.Object.Instantiate(sawBounceEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
			if (flag2 && gameObject3.TryGetComponent<AudioSource>(out var component4))
			{
				component4.enabled = false;
			}
			else
			{
				flag2 = true;
			}
			punched = false;
			punchable = true;
		}
	}

	public Magnet GetTargetMagnet()
	{
		Magnet result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < magnets.Count; i++)
		{
			float num2 = Vector3.Distance(base.transform.position, magnets[i].transform.position);
			if (num2 < num)
			{
				num = num2;
				result = magnets[i];
				if (Vector3.Dot(magnets[i].transform.position - base.transform.position, Quaternion.Euler(0f, 90f, 0f) * rb.velocity.normalized) > 0f)
				{
					magnetRotationDirection = -1;
				}
				else
				{
					magnetRotationDirection = 1;
				}
			}
		}
		return result;
	}

	private void OnCollisionEnter(Collision other)
	{
		if (hit)
		{
			return;
		}
		if ((other.gameObject.layer == 10 || other.gameObject.layer == 11) && (other.gameObject.tag == "Head" || other.gameObject.tag == "Body" || other.gameObject.tag == "Limb" || other.gameObject.tag == "EndLimb" || other.gameObject.tag == "Enemy"))
		{
			TouchEnemy(other.transform);
		}
		else if (enemy && other.gameObject.layer == 2)
		{
			MonoSingleton<NewMovement>.Instance.GetHurt(8, invincible: true);
			hit = true;
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			if (magnets.Count != 0 || (other.gameObject.layer != 8 && other.gameObject.layer != 24))
			{
				return;
			}
			hit = true;
			CancelInvoke("RemoveTime");
			Invoke("RemoveTime", 1f);
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = environmentHitSound;
			aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
			aud.volume = 0.2f;
			aud.Play();
			Breakable component = other.gameObject.GetComponent<Breakable>();
			if (component != null && (component.weak || heated) && !component.precisionOnly)
			{
				component.Break();
			}
			if (other.gameObject.TryGetComponent<Bleeder>(out var component2))
			{
				component2.GetHit(base.transform.position, GoreType.Small);
			}
			if (heated)
			{
				Flammable componentInChildren = other.gameObject.GetComponentInChildren<Flammable>();
				if (componentInChildren != null)
				{
					componentInChildren.Burn(2f);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!sawblade && !hit && (other.gameObject.layer == 10 || other.gameObject.layer == 11) && (other.gameObject.tag == "Head" || other.gameObject.tag == "Body" || other.gameObject.tag == "Limb" || other.gameObject.tag == "EndLimb" || other.gameObject.tag == "Enemy"))
		{
			hit = true;
			TouchEnemy(other.transform);
		}
	}

	private void TouchEnemy(Transform other)
	{
		if (sawblade && multiHitAmount > 1)
		{
			if (!stopped && other.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid)
			{
				if (component.eid.dead)
				{
					HitEnemy(other, component);
				}
				else if (!(sameEnemyHitCooldown > 0f) || !(currentHitEnemy != null) || !(currentHitEnemy == component.eid))
				{
					stopped = true;
					currentMultiHitAmount = multiHitAmount;
					hitTarget = other;
					currentHitEnemy = component.eid;
					originalVelocity = rb.velocity;
					sameEnemyHitCooldown = 0.05f;
				}
			}
		}
		else
		{
			HitEnemy(other);
		}
	}

	private void HitEnemy(Transform other, EnemyIdentifierIdentifier eidid = null)
	{
		if ((!eidid && !other.TryGetComponent<EnemyIdentifierIdentifier>(out eidid)) || !eidid.eid || (enemy && (bool)eidid && (bool)eidid.eid && eidid.eid.enemyType == safeEnemyType) || (sawblade && ((sameEnemyHitCooldown > 0f && currentHitEnemy != null && currentHitEnemy == eidid.eid) || hitLimbs.Contains(other))))
		{
			return;
		}
		if (!sawblade)
		{
			hit = true;
		}
		else if (!eidid.eid.dead)
		{
			sameEnemyHitCooldown = 0.05f;
			currentHitEnemy = eidid.eid;
			hitAmount -= 1f;
		}
		if (aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		aud.clip = enemyHitSound;
		aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
		aud.volume = 0.2f;
		aud.Play();
		if ((bool)eidid && (bool)eidid.eid)
		{
			DamageEnemy(other, eidid.eid);
		}
		if (sawblade)
		{
			if (hitAmount < 1f)
			{
				SawBreak();
			}
			return;
		}
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		rb.isKinematic = true;
		UnityEngine.Object.Destroy(rb);
		base.transform.position += base.transform.forward * -0.5f;
		base.transform.SetParent(other.transform, worldPositionStays: true);
		if (TryGetComponent<TrailRenderer>(out var component))
		{
			component.enabled = false;
		}
		CancelInvoke("RemoveTime");
	}

	private void DamageEnemy(Transform other, EnemyIdentifier eid)
	{
		if (!sawblade)
		{
			eid.hitter = "nail";
		}
		else
		{
			eid.hitter = "sawblade";
		}
		if (!eid.hitterWeapons.Contains(weaponType))
		{
			eid.hitterWeapons.Add(weaponType);
		}
		bool flag = false;
		if (magnets.Count > 0)
		{
			foreach (Magnet magnet in magnets)
			{
				if (magnet.ignoredEids.Contains(eid))
				{
					flag = true;
					break;
				}
			}
		}
		bool dead = eid.dead;
		if (fodderDamageBoost && !eid.dead)
		{
			damage *= GetFodderDamageMultiplier(eid.enemyType);
		}
		if (!nbc)
		{
			eid.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 3000f, base.transform.position, damage * (float)((!punched) ? 1 : 2), tryForExplode: false, 0f, sourceWeapon);
		}
		else if (!nbc.damagedEnemies.Contains(eid))
		{
			eid.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 3000f, base.transform.position, damage * (float)(nbc.nails.Count / 2) * (float)((!punched) ? 1 : 2), tryForExplode: true, 0f, sourceWeapon);
			nbc.damagedEnemies.Add(eid);
		}
		if (!dead && eid.dead && !flag && magnets.Count > 0)
		{
			if (magnets.Count > 1)
			{
				StyleHUD instance = MonoSingleton<StyleHUD>.Instance;
				int points = Mathf.RoundToInt(120f);
				EnemyIdentifier eid2 = eid;
				instance.AddPoints(points, "ultrakill.bipolar", sourceWeapon, eid2);
			}
			else
			{
				StyleHUD instance2 = MonoSingleton<StyleHUD>.Instance;
				int points2 = Mathf.RoundToInt(60f);
				EnemyIdentifier eid2 = eid;
				instance2.AddPoints(points2, "ultrakill.attripator", sourceWeapon, eid2);
			}
		}
		else if (launched)
		{
			if (!dead && eid.dead)
			{
				StyleHUD instance3 = MonoSingleton<StyleHUD>.Instance;
				int points3 = Mathf.RoundToInt(120f);
				EnemyIdentifier eid2 = eid;
				instance3.AddPoints(points3, "ultrakill.nailbombed", sourceWeapon, eid2);
			}
			else if (!eid.dead)
			{
				StyleHUD instance4 = MonoSingleton<StyleHUD>.Instance;
				int points4 = Mathf.RoundToInt(10f);
				EnemyIdentifier eid2 = eid;
				instance4.AddPoints(points4, "ultrakill.nailbombedalive", sourceWeapon, eid2);
			}
		}
		if (!dead && !sawblade)
		{
			eid.nailsAmount++;
			eid.nails.Add(this);
		}
		else if (dead && sawblade)
		{
			hitLimbs.Add(other);
		}
		if (heated)
		{
			Flammable componentInChildren = eid.GetComponentInChildren<Flammable>();
			if (componentInChildren != null)
			{
				componentInChildren.Burn(2f, componentInChildren.burning);
			}
		}
		if (dead)
		{
			_ = magnets.Count;
		}
	}

	public void MagnetCaught(Magnet mag)
	{
		CancelInvoke("RemoveTime");
		launched = false;
		enemy = false;
		if (sawblade)
		{
			punchable = true;
		}
		if (!magnets.Contains(mag))
		{
			magnets.Add(mag);
		}
		if ((bool)nbc)
		{
			nbc.nails.Remove(this);
			nbc = null;
		}
	}

	public void MagnetRelease(Magnet mag)
	{
		CancelInvoke("RemoveTime");
		if (magnets.Contains(mag))
		{
			magnets.Remove(mag);
			if (magnets.Count == 0)
			{
				if (TryGetComponent<SphereCollider>(out var component))
				{
					component.enabled = true;
				}
				launched = true;
			}
		}
		if (magnets.Count == 0)
		{
			Invoke("RemoveTime", 5f * removeTimeMultiplier);
		}
	}

	public void Zap()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			component.material = zapMaterial;
		}
		zapped = true;
	}

	private void RemoveTime()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void MasterRemoveTime()
	{
		RemoveTime();
	}

	public void SawBreak()
	{
		hit = true;
		UnityEngine.Object.Instantiate(sawBreakEffect, base.transform.position, Quaternion.identity);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private float GetFodderDamageMultiplier(EnemyType et)
	{
		return et switch
		{
			EnemyType.Filth => 2f, 
			EnemyType.Schism => 1.5f, 
			EnemyType.Soldier => 1.5f, 
			EnemyType.Stalker => 1.5f, 
			EnemyType.Stray => 2f, 
			_ => 1f, 
		};
	}

	public void ForceCheckSawbladeRicochet()
	{
		if (!rb)
		{
			rb = GetComponent<Rigidbody>();
		}
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			if (!Physics.Raycast(base.transform.position, rb.velocity.normalized, out var hitInfo, 5f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				break;
			}
			if (hitInfo.transform.TryGetComponent<Breakable>(out var component) && component.weak)
			{
				component.Break();
				return;
			}
			base.transform.position = hitInfo.point;
			if (bounceToSurfaceNormal)
			{
				rb.velocity = hitInfo.normal * rb.velocity.magnitude;
			}
			else
			{
				rb.velocity = Vector3.Reflect(rb.velocity.normalized, hitInfo.normal) * rb.velocity.magnitude;
			}
			hitAmount -= 0.125f;
			GameObject gameObject = UnityEngine.Object.Instantiate(sawBounceEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
			if (flag && gameObject.TryGetComponent<AudioSource>(out var component2))
			{
				component2.enabled = false;
			}
			else
			{
				flag = true;
			}
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, 1.5f, LayerMaskDefaults.Get(LMD.Enemies));
		if (array.Length != 0)
		{
			TouchEnemy(array[0].transform);
		}
	}
}
