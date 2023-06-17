using ULTRAKILL.Cheats;
using UnityEngine;

public class Harpoon : MonoBehaviour
{
	[SerializeField]
	private Magnet magnet;

	public bool drill;

	private bool drilling;

	private float drillCooldown;

	private bool hit;

	private bool stopped;

	private bool punched;

	public float damage;

	private float damageLeft;

	private AudioSource aud;

	public AudioClip environmentHitSound;

	public AudioClip enemyHitSound;

	private Rigidbody rb;

	private EnemyIdentifierIdentifier target;

	public AudioSource drillSound;

	private AudioSource currentDrillSound;

	public int drillHits;

	private int drillHitsLeft;

	private Vector3 startPosition;

	[SerializeField]
	private GameObject breakEffect;

	private FixedJoint fj;

	private TrailRenderer tr;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		tr = GetComponent<TrailRenderer>();
		damageLeft = damage;
		if (drill)
		{
			drillHitsLeft = drillHits;
		}
		Invoke("DestroyIfNotHit", 5f);
		Invoke("MasterDestroy", 30f);
		Invoke("SlowUpdate", 2f);
		startPosition = base.transform.position;
	}

	private void SlowUpdate()
	{
		if (Vector3.Distance(startPosition, base.transform.position) > 999f)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Invoke("SlowUpdate", 2f);
		}
	}

	private void Update()
	{
		if (!stopped && !punched && rb.velocity.magnitude > 1f)
		{
			base.transform.LookAt(base.transform.position + rb.velocity);
		}
		else if (drilling)
		{
			base.transform.Rotate(Vector3.forward, 14400f * Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (!stopped || !drilling || !target)
		{
			return;
		}
		if (drillCooldown != 0f)
		{
			drillCooldown = Mathf.MoveTowards(drillCooldown, 0f, Time.deltaTime);
			return;
		}
		drillCooldown = 0.05f;
		if ((bool)target.eid)
		{
			target.eid.hitter = "drill";
			target.eid.DeliverDamage(target.gameObject, Vector3.zero, base.transform.position, 0.0625f, tryForExplode: false);
		}
		if ((bool)currentDrillSound)
		{
			currentDrillSound.pitch = 1.5f - (float)drillHitsLeft / (float)drillHits / 2f;
		}
		if (drillHitsLeft > 0)
		{
			drillHitsLeft--;
		}
		else if (!PauseTimedBombs.Paused)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if ((bool)target && (bool)target.eid && (bool)magnet && target.eid.stuckMagnets.Contains(magnet))
		{
			target.eid.stuckMagnets.Remove(magnet);
		}
		if (drill)
		{
			Object.Instantiate(breakEffect, base.transform.position, base.transform.rotation);
		}
	}

	private void OnEnable()
	{
		if (stopped && (bool)target && (bool)target.eid && drill)
		{
			target.eid.drillers.Add(this);
		}
	}

	private void OnDisable()
	{
		if (stopped && (bool)target && (bool)target.eid && drill && target.eid.drillers.Contains(this))
		{
			target.eid.drillers.Remove(this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		GoreZone componentInParent = other.GetComponentInParent<GoreZone>();
		if (!hit && (other.gameObject.layer == 10 || other.gameObject.layer == 11) && (other.gameObject.CompareTag("Armor") || other.gameObject.CompareTag("Head") || other.gameObject.CompareTag("Body") || other.gameObject.CompareTag("Limb") || other.gameObject.CompareTag("EndLimb")))
		{
			if (!other.TryGetComponent<EnemyIdentifierIdentifier>(out var component) || !component.eid || ((bool)target && (bool)target.eid && component.eid == target.eid) || (drill && component.eid.harpooned) || ((bool)magnet && component.eid.dead && component.eid.enemyType != EnemyType.MaliciousFace))
			{
				return;
			}
			target = component;
			hit = true;
			EnemyIdentifier eid = target.eid;
			eid.hitter = "harpoon";
			float health = eid.health;
			eid.DeliverDamage(other.gameObject, Vector3.zero, base.transform.position, damageLeft, tryForExplode: false);
			if (drill)
			{
				eid.drillers.Add(this);
			}
			if (health < damageLeft)
			{
				damageLeft -= health;
			}
			if (other.gameObject.layer == 10)
			{
				fj = base.gameObject.AddComponent<FixedJoint>();
				fj.connectedBody = other.gameObject.GetComponentInParent<Rigidbody>();
				if (componentInParent != null)
				{
					base.transform.SetParent(componentInParent.transform, worldPositionStays: true);
				}
			}
			else
			{
				rb.velocity = Vector3.zero;
				rb.useGravity = false;
				rb.constraints = RigidbodyConstraints.FreezeAll;
				base.transform.SetParent(other.transform, worldPositionStays: true);
			}
			if (!magnet && eid.dead && !eid.harpooned && other.gameObject.layer == 10 && (!eid.machine || !eid.machine.specialDeath))
			{
				eid.harpooned = true;
				other.gameObject.transform.position = base.transform.position;
				rb?.AddForce(base.transform.forward, ForceMode.VelocityChange);
				if (drill)
				{
					hit = false;
				}
			}
			else
			{
				stopped = true;
				if (drill)
				{
					drilling = true;
					currentDrillSound = Object.Instantiate(drillSound, base.transform.position, base.transform.rotation);
					currentDrillSound.transform.SetParent(base.transform, worldPositionStays: true);
				}
				rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
				tr.emitting = false;
				TimeBomb component2 = GetComponent<TimeBomb>();
				if (component2 != null)
				{
					component2.StartCountdown();
				}
				if (magnet != null)
				{
					magnet.onEnemy = true;
					magnet.ignoredEids.Add(eid);
					magnet.ExitEnemy(eid);
					if (eid.enemyType != EnemyType.FleshPrison)
					{
						magnet.transform.position = other.bounds.center;
					}
					if (!eid.stuckMagnets.Contains(magnet))
					{
						eid.stuckMagnets.Add(magnet);
					}
					if (!component.eid.dead)
					{
						Breakable[] componentsInChildren = GetComponentsInChildren<Breakable>();
						if (componentsInChildren.Length != 0)
						{
							Breakable[] array = componentsInChildren;
							for (int i = 0; i < array.Length; i++)
							{
								Object.Destroy(array[i].gameObject);
							}
						}
					}
				}
			}
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = enemyHitSound;
			aud.pitch = Random.Range(0.9f, 1.1f);
			aud.volume = 0.4f;
			aud.Play();
		}
		else
		{
			if (stopped || (other.gameObject.layer != 8 && other.gameObject.layer != 24))
			{
				return;
			}
			if (drill && !hit)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			stopped = true;
			hit = true;
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			rb.isKinematic = true;
			if (other.gameObject.CompareTag("Door") || other.gameObject.CompareTag("Moving") || ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(other.transform)))
			{
				Rigidbody component3 = other.gameObject.GetComponent<Rigidbody>();
				if ((bool)component3)
				{
					base.gameObject.AddComponent<FixedJoint>().connectedBody = component3;
					rb.isKinematic = false;
				}
				hit = true;
				base.transform.SetParent(other.transform, worldPositionStays: true);
				if ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(other.transform) && other.transform.TryGetComponent<ScrollingTexture>(out var component4))
				{
					component4.attachedObjects.Add(base.transform);
					if (TryGetComponent<BoxCollider>(out var component5))
					{
						component4.specialScrollers.Add(new WaterDryTracker(base.transform, component5.ClosestPoint(other.ClosestPoint(base.transform.position + base.transform.forward * component5.size.z * base.transform.lossyScale.z)) - base.transform.position));
					}
				}
			}
			else if ((bool)componentInParent)
			{
				base.transform.SetParent(componentInParent.transform, worldPositionStays: true);
			}
			else
			{
				GoreZone[] array2 = Object.FindObjectsOfType<GoreZone>();
				if (array2 != null && array2.Length != 0)
				{
					GoreZone goreZone = array2[0];
					if (array2.Length > 1)
					{
						for (int j = 1; j < array2.Length; j++)
						{
							if (array2[j].gameObject.activeInHierarchy && Vector3.Distance(goreZone.transform.position, base.transform.position) > Vector3.Distance(array2[j].transform.position, base.transform.position))
							{
								goreZone = array2[j];
							}
						}
					}
					base.transform.SetParent(goreZone.transform, worldPositionStays: true);
				}
			}
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = environmentHitSound;
			aud.pitch = Random.Range(0.9f, 1.1f);
			aud.volume = 0.4f;
			aud.Play();
			tr.emitting = false;
			TimeBomb component6 = GetComponent<TimeBomb>();
			if (component6 != null)
			{
				component6.StartCountdown();
			}
		}
	}

	public void Punched()
	{
		hit = false;
		stopped = false;
		drilling = false;
		punched = true;
		damageLeft = damage;
		drillHitsLeft = drillHits;
		CancelInvoke("DestroyIfNotHit");
		Invoke("DestroyIfNotHit", 5f);
		CancelInvoke("MasterDestroy");
		Invoke("MasterDestroy", 30f);
		CancelInvoke("DestroyIfOnCorpse");
		rb.isKinematic = false;
		rb.useGravity = false;
		rb.AddForce(base.transform.forward * 150f, ForceMode.VelocityChange);
		aud.Stop();
		rb.constraints = RigidbodyConstraints.None;
		base.transform.SetParent(null, worldPositionStays: true);
		if ((bool)tr)
		{
			tr.emitting = true;
		}
		if ((bool)target && (bool)target.eid)
		{
			target.eid.drillers.Remove(this);
			target.eid.DeliverDamage(target.gameObject, base.transform.forward * 150f, base.transform.position, 4f, tryForExplode: true);
			if ((bool)fj)
			{
				Object.Destroy(fj);
			}
			if ((bool)currentDrillSound)
			{
				Object.Destroy(currentDrillSound);
			}
		}
	}

	private void DestroyIfNotHit()
	{
		if (!hit && !PauseTimedBombs.Paused)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void MasterDestroy()
	{
		if (!PauseTimedBombs.Paused)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void DelayedDestroyIfOnCorpse()
	{
		Invoke("DestroyIfOnCorpse", 1f);
	}

	private void DestroyIfOnCorpse()
	{
		if ((bool)target && (!target.eid || target.eid.dead))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
