using System.Collections.Generic;
using UnityEngine;

public class MassSpear : MonoBehaviour
{
	private LineRenderer lr;

	private Rigidbody rb;

	public bool hitPlayer;

	private List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

	public bool beenStopped;

	private bool returning;

	private bool deflected;

	public Transform originPoint;

	private NewMovement nmov;

	public float spearHealth;

	private int difficulty;

	public GameObject breakMetalSmall;

	private AudioSource aud;

	public AudioClip hit;

	public AudioClip stop;

	private Mass mass;

	public float speedMultiplier = 1f;

	public float damageMultiplier = 1f;

	private void Start()
	{
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		lr = GetComponentInChildren<LineRenderer>();
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		mass = originPoint.GetComponentInParent<Mass>();
		Invoke("CheckForDistance", 3f / speedMultiplier);
		if (difficulty == 1)
		{
			rb.AddForce(base.transform.forward * 75f * speedMultiplier, ForceMode.VelocityChange);
		}
		if (difficulty == 2)
		{
			rb.AddForce(base.transform.forward * 200f * speedMultiplier, ForceMode.VelocityChange);
		}
		else if (difficulty >= 3)
		{
			rb.AddForce(base.transform.forward * 250f * speedMultiplier, ForceMode.VelocityChange);
		}
	}

	private void OnDisable()
	{
		if (!returning)
		{
			Return();
		}
	}

	private void Update()
	{
		if (originPoint != null && !originPoint.gameObject.activeInHierarchy)
		{
			lr.SetPosition(0, originPoint.position);
			lr.SetPosition(1, lr.transform.position);
			if (returning)
			{
				if (!originPoint || !originPoint.parent || !originPoint.parent.gameObject.activeInHierarchy)
				{
					Object.Destroy(base.gameObject);
					return;
				}
				base.transform.rotation = Quaternion.LookRotation(base.transform.position - originPoint.position, Vector3.up);
				rb.velocity = base.transform.forward * -100f * speedMultiplier;
				if (Vector3.Distance(base.transform.position, originPoint.position) < 1f)
				{
					if (mass != null)
					{
						mass.SpearReturned();
					}
					Object.Destroy(base.gameObject);
				}
			}
			else if (deflected)
			{
				base.transform.LookAt(originPoint.position);
				rb.velocity = base.transform.forward * 100f * speedMultiplier;
				if (!(Vector3.Distance(base.transform.position, originPoint.position) < 1f) || !(mass != null))
				{
					return;
				}
				mass.SpearReturned();
				mass.GetComponent<Statue>().GetHurt(mass.tailEnd.GetChild(0).gameObject, Vector3.zero, 30f * damageMultiplier, 0f, originPoint.position);
				BloodsplatterManager instance = MonoSingleton<BloodsplatterManager>.Instance;
				EnemyIdentifier component = mass.GetComponent<EnemyIdentifier>();
				for (int i = 0; i < 3; i++)
				{
					GameObject gore = instance.GetGore(GoreType.Head, component.underwater, component.sandified, component.blessed);
					gore.transform.position = mass.tailEnd.GetChild(0).position;
					GoreZone componentInParent = GetComponentInParent<GoreZone>();
					if ((bool)componentInParent)
					{
						gore.transform.SetParent(componentInParent.goreZone);
					}
					else if (base.transform.parent != null)
					{
						gore.transform.SetParent(base.transform.parent);
					}
					else
					{
						gore.transform.SetParent(mass.transform);
					}
				}
				mass.SpearParried();
				Object.Destroy(base.gameObject);
			}
			else if (hitPlayer && !returning)
			{
				if (nmov.hp <= 0)
				{
					Return();
					Object.Destroy(base.gameObject);
				}
				if (spearHealth > 0f)
				{
					spearHealth = Mathf.MoveTowards(spearHealth, 0f, Time.deltaTime);
				}
				else if (spearHealth <= 0f)
				{
					Return();
				}
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!beenStopped)
		{
			if (!hitPlayer && other.gameObject.tag == "Player")
			{
				hitPlayer = true;
				nmov = other.GetComponent<NewMovement>();
				nmov.GetHurt(Mathf.RoundToInt(25f * damageMultiplier), invincible: true);
				nmov.slowMode = true;
				base.transform.position = other.transform.position;
				base.transform.SetParent(other.transform, worldPositionStays: true);
				rb.velocity = Vector3.zero;
				rb.useGravity = false;
				rb.isKinematic = true;
				beenStopped = true;
				GetComponent<CapsuleCollider>().radius *= 0.1f;
				aud.pitch = 1f;
				aud.clip = hit;
				aud.Play();
			}
			else if (other.gameObject.layer == 8 || other.gameObject.layer == 24)
			{
				beenStopped = true;
				rb.velocity = Vector3.zero;
				rb.useGravity = false;
				base.transform.position += base.transform.forward * 2f;
				Invoke("Return", 2f / speedMultiplier);
				aud.pitch = 1f;
				aud.clip = stop;
				aud.Play();
			}
		}
	}

	public void GetHurt(float damage)
	{
		Object.Instantiate(breakMetalSmall, base.transform.position, Quaternion.identity);
		spearHealth -= damage / 2f;
	}

	public void Deflected()
	{
		deflected = true;
		GetComponent<Collider>().enabled = false;
	}

	private void Return()
	{
		if (hitPlayer)
		{
			nmov.slowMode = false;
			base.transform.SetParent(null, worldPositionStays: true);
			rb.isKinematic = false;
		}
		if (base.gameObject.activeInHierarchy)
		{
			aud.clip = stop;
			aud.pitch = 1f;
			aud.Play();
		}
		returning = true;
		beenStopped = true;
	}

	private void CheckForDistance()
	{
		if (!returning && !beenStopped && !hitPlayer && !deflected)
		{
			returning = true;
			beenStopped = true;
			base.transform.position = originPoint.position;
		}
	}
}
