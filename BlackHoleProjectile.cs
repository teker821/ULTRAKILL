using System.Collections.Generic;
using UnityEngine;

public class BlackHoleProjectile : MonoBehaviour
{
	private Rigidbody rb;

	public float speed;

	private Light bhlight;

	private float targetRange;

	private RaycastHit rhit;

	public LayerMask lmask;

	private AudioSource aud;

	public GameObject lightningBolt;

	public GameObject lightningBolt2;

	private Transform aura;

	public Material additive;

	private bool activated;

	private bool collapsing;

	private float power;

	private StyleCalculator scalc;

	private int killAmount;

	public List<EnemyIdentifier> shootList = new List<EnemyIdentifier>();

	private List<Rigidbody> caughtList = new List<Rigidbody>();

	public bool enemy;

	public EnemyType safeType;

	private NewMovement nmov;

	private Collider col;

	[HideInInspector]
	public bool fadingIn;

	private Vector3 origScale;

	public GameObject spawnEffect;

	public GameObject explosionEffect;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		bhlight = GetComponent<Light>();
		targetRange = Random.Range(0, 15);
		aura = base.transform.GetChild(0);
		aud = GetComponent<AudioSource>();
		Invoke("ShootRandomLightning", Random.Range(0.5f, 1.5f));
		nmov = MonoSingleton<NewMovement>.Instance;
		col = GetComponent<Collider>();
		if (enemy && !activated)
		{
			col.enabled = false;
		}
	}

	private void FixedUpdate()
	{
		if (!enemy)
		{
			if (!activated)
			{
				rb.velocity = base.transform.forward * speed;
			}
		}
		else if (!collapsing && activated)
		{
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(nmov.transform.position - base.transform.position), Time.fixedDeltaTime * 10f * speed);
			rb.velocity = base.transform.forward * speed;
		}
	}

	private void OnDisable()
	{
		CancelInvoke("ShootRandomLightning");
	}

	private void OnEnable()
	{
		ShootRandomLightning();
	}

	private void Update()
	{
		if (bhlight.range != targetRange)
		{
			bhlight.range = Mathf.MoveTowards(bhlight.range, targetRange, 100f * Time.deltaTime);
		}
		else if (activated)
		{
			targetRange = Random.Range(10, 20);
		}
		else
		{
			targetRange = Random.Range(0, 15);
		}
		if (activated && !enemy)
		{
			aura.transform.localPosition = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
		}
		else
		{
			aura.transform.localPosition = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
		}
		if (fadingIn)
		{
			base.transform.localScale = Vector3.MoveTowards(base.transform.localScale, origScale, Time.deltaTime * origScale.magnitude);
			aud.pitch = 1f - Vector3.Distance(base.transform.localScale, origScale) / origScale.magnitude;
			if (base.transform.localScale == origScale)
			{
				aud.pitch = 1f;
				fadingIn = false;
			}
		}
		if (collapsing)
		{
			if (aud.pitch > 0f)
			{
				aud.pitch -= Time.deltaTime;
			}
			else if (aud.pitch != 0f)
			{
				aud.pitch = 0f;
			}
			foreach (Rigidbody caught in caughtList)
			{
				if (caught != null)
				{
					caught.transform.position = base.transform.position;
				}
			}
			if (base.transform.localScale.x > 0f)
			{
				base.transform.localScale -= Vector3.one * Time.deltaTime;
			}
			else
			{
				Explode();
			}
		}
		else
		{
			if (!activated)
			{
				return;
			}
			if (!enemy)
			{
				aud.pitch += Time.deltaTime / 2f;
			}
			if (power < 3f)
			{
				power += Time.deltaTime;
			}
			if (caughtList.Count == 0)
			{
				return;
			}
			List<Rigidbody> list = new List<Rigidbody>();
			foreach (Rigidbody caught2 in caughtList)
			{
				if (caught2 == null)
				{
					list.Add(caught2);
					continue;
				}
				if (Vector3.Distance(caught2.transform.position, base.transform.position) < 9f)
				{
					caught2.transform.position = Vector3.MoveTowards(caught2.transform.position, base.transform.position, power * Time.deltaTime * (10f - Vector3.Distance(caught2.transform.position, base.transform.position)));
				}
				else
				{
					caught2.transform.position = Vector3.MoveTowards(caught2.transform.position, base.transform.position, power * Time.deltaTime);
				}
				if (Vector3.Distance(caught2.transform.position, base.transform.position) < 1f)
				{
					CharacterJoint component = caught2.GetComponent<CharacterJoint>();
					if (component != null)
					{
						Object.Destroy(component);
					}
					caught2.GetComponent<Collider>().enabled = false;
				}
				if (!(Vector3.Distance(caught2.transform.position, base.transform.position) < 0.25f))
				{
					continue;
				}
				List<Rigidbody> list2 = new List<Rigidbody>();
				list.Add(caught2);
				caught2.useGravity = false;
				caught2.velocity = Vector3.zero;
				caught2.isKinematic = true;
				caught2.transform.SetParent(base.transform);
				caught2.transform.localPosition = Vector3.zero;
				if (list2.Count != 0)
				{
					foreach (Rigidbody item in list2)
					{
						caughtList.Remove(item);
					}
				}
				list2.Clear();
			}
			if (list.Count == 0)
			{
				return;
			}
			foreach (Rigidbody item2 in list)
			{
				caughtList.Remove(item2);
			}
		}
	}

	private void ShootRandomLightning()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		int num = Random.Range(2, 6);
		for (int i = 0; i < num; i++)
		{
			if (Physics.Raycast(base.transform.position, Random.insideUnitSphere.normalized, out rhit, 8f * base.transform.localScale.x, lmask))
			{
				LineRenderer component = Object.Instantiate(lightningBolt, base.transform.position, base.transform.rotation).GetComponent<LineRenderer>();
				component.SetPosition(0, base.transform.position);
				component.SetPosition(1, rhit.point);
				component.widthMultiplier = base.transform.localScale.x * 2f;
			}
		}
		if (!activated || enemy)
		{
			Invoke("ShootRandomLightning", Random.Range(0.5f, 3f));
		}
	}

	private void ShootTargetLightning()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (shootList.Count != 0)
		{
			List<EnemyIdentifier> list = new List<EnemyIdentifier>();
			foreach (EnemyIdentifier shoot in shootList)
			{
				if (enemy && (shoot.enemyType == safeType || EnemyIdentifier.CheckHurtException(safeType, shoot.enemyType)))
				{
					continue;
				}
				LineRenderer component = Object.Instantiate(lightningBolt2, base.transform.position, base.transform.rotation).GetComponent<LineRenderer>();
				component.SetPosition(0, base.transform.position);
				component.SetPosition(1, shoot.transform.position);
				if (!enemy)
				{
					shoot.hitter = "secret";
				}
				else
				{
					shoot.hitter = "enemy";
				}
				shoot.DeliverDamage(shoot.gameObject, Vector3.zero, shoot.transform.position, 1f, tryForExplode: false);
				if (shoot.dead)
				{
					list.Add(shoot);
					Rigidbody[] componentsInChildren = shoot.GetComponentsInChildren<Rigidbody>();
					foreach (Rigidbody item in componentsInChildren)
					{
						caughtList.Add(item);
					}
				}
			}
			if (list.Count != 0)
			{
				foreach (EnemyIdentifier item2 in list)
				{
					shootList.Remove(item2);
				}
				list.Clear();
			}
		}
		if (!enemy)
		{
			ShootRandomLightning();
		}
		Invoke("ShootTargetLightning", 0.5f);
	}

	public void Activate()
	{
		if (fadingIn)
		{
			base.transform.localScale = origScale;
		}
		if ((bool)spawnEffect)
		{
			Object.Instantiate(spawnEffect, base.transform.position, Quaternion.identity).transform.localScale = base.transform.localScale * 5f;
		}
		activated = true;
		if (!rb)
		{
			rb = GetComponent<Rigidbody>();
		}
		rb.velocity = Vector3.zero;
		base.transform.GetChild(0).GetComponent<SpriteRenderer>().material = additive;
		GetComponentInChildren<ParticleSystem>().Play();
		ShootTargetLightning();
		if (!enemy)
		{
			Invoke("Collapse", 3f);
		}
		else if ((bool)col)
		{
			col.enabled = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (enemy && other.gameObject == nmov.gameObject)
		{
			Explode();
			if (nmov.hp > 10)
			{
				nmov.GetHurt(nmov.hp - 1, invincible: true);
				nmov.ForceAntiHP(99f);
			}
			else
			{
				nmov.GetHurt(10, invincible: true);
			}
		}
	}

	private void Collapse()
	{
		collapsing = true;
	}

	public void FadeIn()
	{
		if (origScale == Vector3.zero)
		{
			origScale = base.transform.localScale;
		}
		base.transform.localScale = Vector3.zero;
		fadingIn = true;
		Debug.Log("FadeIn");
	}

	public void Explode()
	{
		Debug.Log("Explode. FadeIn: " + fadingIn);
		Object.Instantiate(explosionEffect, base.transform.position, Quaternion.identity).transform.localScale = base.transform.localScale * 5f;
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
		{
			if (!((bool)enemyIdentifierIdentifier & (bool)enemyIdentifierIdentifier.eid) || (!(enemyIdentifierIdentifier.gameObject.tag == "EndLimb") && !(enemyIdentifierIdentifier.gameObject.tag == "Head")))
			{
				continue;
			}
			if (!enemy)
			{
				enemyIdentifierIdentifier.eid.hitter = "secret";
			}
			else
			{
				enemyIdentifierIdentifier.eid.hitter = "enemy";
			}
			enemyIdentifierIdentifier.eid.DeliverDamage(enemyIdentifierIdentifier.gameObject, Vector3.zero, enemyIdentifierIdentifier.gameObject.transform.position, 100f, tryForExplode: false);
			if (enemyIdentifierIdentifier.eid.exploded)
			{
				continue;
			}
			enemyIdentifierIdentifier.eid.exploded = true;
			if (!enemy)
			{
				if (scalc == null)
				{
					scalc = MonoSingleton<StyleCalculator>.Instance;
				}
				killAmount++;
				scalc.shud.AddPoints(50 - killAmount * 10, "ultrakill.compressed");
				scalc.HitCalculator("", "", "", dead: true);
			}
		}
		Object.Destroy(base.gameObject);
	}
}
